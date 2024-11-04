using System.Collections.Generic;
using System.Threading.Tasks;
using FMODUnity;
using UnityEngine;
using static StatusUpdatePacket;

public class WorldCombat : MonoBehaviour
{
    [SerializeField] private List<Hit> _hits;
    private EventProcessor _eventProcessor;
    private WorldSpawner _worldSpawner;

    private static WorldCombat _instance;
    public static WorldCombat Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        _hits = new List<Hit>();
        _eventProcessor = EventProcessor.Instance;
        _worldSpawner = GetComponent<WorldSpawner>();
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void FixedUpdate()
    {
        ApplyHits();
    }

    private void ApplyHits()
    {
        float now = Time.time;
        for (int i = _hits.Count - 1; i >= 0; i--)
        {
            Hit hit = _hits[i];
            if (now >= hit.HitTime)
            {
                _hits.RemoveAt(i);
                InflictAttack(hit.Attacker, hit.Target, hit);
            }
        }
    }

    public void InflictAttack(Entity attacker, Entity target, Hit hit)
    {
        ApplyDamage(target, hit);

        if (hit.isMiss())
        {
            attacker.ReferenceHolder.AudioHandler.PlaySwishSound();
            return;
        }

        // Instantiate hit particle
        ParticleManager.Instance.SpawnHitParticle(attacker, target, hit);
    }

    private void ApplyDamage(Entity target, Hit hit)
    {
        // Apply damage to target
        target.Combat.ApplyDamage(hit);
    }

    public Task OnMagicSkillUse(MagicSkillUsePacket packet)
    {
        return _worldSpawner.ExecuteWithEntitiesAsync(packet.ObjectId, packet.TargetId, (targeter, targeted) =>
        {
            EntityCastSkill(targeter, packet.SkillId);
        });
    }


    public void EntityCastSkill(Entity entity, int skillId)
    {
        Skill skill = SkillTable.Instance.GetSkill(skillId);
        CastSkill(entity, skill);
    }

    public void EntityCastSkill(Entity entity, Skill skill)
    {
        CastSkill(entity, skill);
    }

    private void CastSkill(Entity entity, Skill skill)
    {
        // Spawn particle
        ParticleManager.Instance.SpawnSkillParticles(entity, skill);

        // Cast skill sound
        if (skill.SkillSoundgrp == null || skill.SkillSoundgrp.SpellEffectSounds == null || skill.SkillSoundgrp.SpellEffectSounds.Length == 0)
        {
            Debug.LogWarning($"Skill {skill.SkillId} doesnt have any SoundGrp or can't find sound EventReference.");
            return;
        }

        EventReference soundReference = skill.SkillSoundgrp.SpellEffectSounds[0].SoundEvent;
        AudioManager.Instance.PlaySound(soundReference, entity.transform.position);
    }

    public Task UpdateEntityTarget(int id, int targetId, Vector3 position)
    {
        return _worldSpawner.ExecuteWithEntitiesAsync(id, targetId, (targeter, targeted) =>
        {
            ((NetworkEntityReferenceHolder)targeter.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
            targeter.Combat.TargetId = targetId;
            targeter.Combat.Target = targeted;
        });
    }

    public Task UpdateMyTarget(int id, int targetId)
    {
        return _worldSpawner.ExecuteWithEntitiesAsync(id, targetId, (targeter, targeted) =>
        {
            targeter.Combat.TargetId = targetId;
            targeter.Combat.Target = targeted;
        });
    }

    public Task UnsetEntityTarget(int id)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            e.Combat.TargetId = -1;
            e.Combat.Target = null;
        });
    }

    public Task StatusUpdate(int id, List<StatusUpdatePacket.Attribute> attributes)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            StatusUpdate(e, attributes);
            if (e == PlayerEntity.Instance)
            {
                // InventoryWindow.Instance.RefreshWeight(((PlayerStats)e.Stats).CurrWeight, ((PlayerStats)e.Stats).MaxWeight);
                CharacterInfoWindow.Instance.UpdateValues();
            }
        });
    }

    public Task EntityDied(int id, bool toVillageAllowed, bool toClanHallAllowed, bool toCastleAllowed, bool toSiegeHQAllowed, bool sweepable, bool fixedResAllowed)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                PlayerController.Instance.ResetDestination(false);
                RestartLocationWindow.Instance.ShowWindowWithParams(toVillageAllowed, toClanHallAllowed, toCastleAllowed, toSiegeHQAllowed, fixedResAllowed);
            }

            e.ReferenceHolder.Combat.OnDeath();
        });
    }

    public Task EntityRevived(int id)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            e.ReferenceHolder.Combat.OnRevive();
        });
    }

    public Task EntityAttacks(Vector3 attackerPosition, int sender, Hit hit)
    {
        Debug.LogWarning("ENTITY ATTACK?");
        return _worldSpawner.ExecuteWithEntitiesAsync(sender, hit.TargetId, (senderEntity, targetEntity) =>
        {
            //TODO: Handle AOE

            Debug.LogWarning("ENTITY ATTACK SUCCESS");
            hit.Attacker = senderEntity;
            hit.Target = targetEntity;
            hit.HitTime = Time.time + senderEntity.AnimationController.PAtkSpd / 2f / 1000f;
            _hits.Add(hit);

            // Debug.Log($"Hit scheduled in {senderEntity.AnimationController.PAtkSpd / 2f} ms - Now: {Time.time} - HitTime: {hit.HitTime}");

            if (sender != GameClient.Instance.CurrentPlayerId)
            {
                NetworkEntityReferenceHolder referenceHolder = (NetworkEntityReferenceHolder)senderEntity.ReferenceHolder;

                // Update attacker target 
                if (referenceHolder.Combat.Target != targetEntity)
                {
                    referenceHolder.Combat.Target = targetEntity;
                    referenceHolder.Combat.AttackTarget = targetEntity;
                }

                Debug.LogWarning("Attacker position: " + attackerPosition);
                referenceHolder.NetworkTransformReceive.SetNewPosition(attackerPosition, false);
                referenceHolder.NetworkCharacterControllerReceive.ResetDestination();
            }
            else
            {
                // NetworkTransformShare.Instance.SharePosition();
                PlayerTransformReceive.Instance.SetNewPosition(attackerPosition);
            }

            // Update attacked target 
            if (hit.TargetId != GameClient.Instance.CurrentPlayerId)
            {
                if (((NetworkEntityReferenceHolder)targetEntity.ReferenceHolder).Combat.Target == null)
                {
                    ((NetworkEntityReferenceHolder)targetEntity.ReferenceHolder).Combat.Target = senderEntity;
                    ((NetworkEntityReferenceHolder)targetEntity.ReferenceHolder).Combat.AttackTarget = senderEntity;
                }
            }

            senderEntity.OnStopMoving();
            senderEntity.AttackTargetOnce();
            // InflictAttack(senderEntity, targetEntity, hit);
        });
    }

    public void StatusUpdate(Entity entity, List<Attribute> attributes)
    {
        // Debug.Log("Word combat: Status update");
        Status status = entity.Status;
        Stats stats = entity.Stats;

        foreach (Attribute attribute in attributes)
        {
            switch ((AttributeType)attribute.id)
            {
                case AttributeType.LEVEL:
                    stats.Level = attribute.value;
                    break;
                case AttributeType.EXP:
                    ((PlayerStats)stats).Exp = attribute.value;
                    break;
                case AttributeType.STR:
                    ((PlayerStats)stats).Str = (byte)attribute.value;
                    break;
                case AttributeType.DEX:
                    ((PlayerStats)stats).Dex = (byte)attribute.value;
                    break;
                case AttributeType.CON:
                    ((PlayerStats)stats).Con = (byte)attribute.value;
                    break;
                case AttributeType.INT:
                    ((PlayerStats)stats).Int = (byte)attribute.value;
                    break;
                case AttributeType.WIT:
                    ((PlayerStats)stats).Wit = (byte)attribute.value;
                    break;
                case AttributeType.MEN:
                    ((PlayerStats)stats).Men = (byte)attribute.value;
                    break;
                case AttributeType.CUR_HP:
                    status.Hp = attribute.value;
                    break;
                case AttributeType.MAX_HP:
                    stats.MaxHp = attribute.value;
                    break;
                case AttributeType.CUR_MP:
                    status.Mp = attribute.value;
                    break;
                case AttributeType.MAX_MP:
                    stats.MaxMp = attribute.value;
                    break;
                case AttributeType.SP:
                    ((PlayerStats)stats).Sp = attribute.value;
                    break;
                case AttributeType.CUR_LOAD:
                    ((PlayerStats)stats).CurrWeight = attribute.value;
                    break;
                case AttributeType.MAX_LOAD:
                    ((PlayerStats)stats).MaxWeight = attribute.value;
                    break;
                case AttributeType.P_ATK:
                    ((PlayerStats)stats).PAtk = attribute.value;
                    break;
                case AttributeType.ATK_SPD:
                    stats.PAtkSpd = attribute.value;
                    entity.UpdatePAtkSpeed(stats.PAtkSpd);
                    break;
                case AttributeType.P_DEF:
                    ((PlayerStats)stats).PDef = attribute.value;
                    break;
                case AttributeType.P_EVASION:
                    ((PlayerStats)stats).PEvasion = attribute.value;
                    break;
                case AttributeType.P_ACCURACY:
                    ((PlayerStats)stats).PAccuracy = attribute.value;
                    break;
                case AttributeType.P_CRITICAL:
                    ((PlayerStats)stats).PCritical = attribute.value;
                    break;
                case AttributeType.M_EVASION:
                    ((PlayerStats)stats).MEvasion = attribute.value;
                    break;
                case AttributeType.M_ACCURACY:
                    ((PlayerStats)stats).MAccuracy = attribute.value;
                    break;
                case AttributeType.M_CRITICAL:
                    ((PlayerStats)stats).MCritical = attribute.value;
                    break;
                case AttributeType.M_ATK:
                    ((PlayerStats)stats).MAtk = attribute.value;
                    break;
                case AttributeType.CAST_SPD:
                    stats.MAtkSpd = attribute.value;
                    entity.UpdateMAtkSpeed(stats.MAtkSpd);
                    break;
                case AttributeType.M_DEF:
                    ((PlayerStats)stats).MDef = attribute.value;
                    break;
                case AttributeType.PVP_FLAG:
                    break;
                case AttributeType.KARMA:
                    break;
                case AttributeType.CUR_CP:
                    ((PlayerStatus)status).Cp = attribute.value;
                    break;
                case AttributeType.MAX_CP:
                    stats.MaxCp = attribute.value;
                    break;
            }
        }
    }

    public float GetRealAttackRange(Entity attacker, Entity target)
    {
        // return attacker.Appearance.CollisionRadius + target.Appearance.CollisionRadius + attacker.Stats.AttackRange;
        // return attacker.Appearance.CollisionRadius + target.Appearance.CollisionRadius;
        return attacker.Stats.AttackRange;
    }

    public void ExAutoSoulshotReceived(int itemId, bool enable)
    {
        _eventProcessor.QueueEvent(() => PlayerShortcuts.Instance.ToggleShortcutItem(itemId, enable));
    }
}
using System.Threading;
using UnityEngine;

public class NpcSpawner : EntitySpawnStrategy<Appearance, Stats, NpcStatus>
{
    private readonly Transform _npcsContainer;
    private readonly Transform _monstersContainer;
    private readonly GameObject _npcPlaceHolder;
    private readonly GameObject _monsterPlaceholder;

    public NpcSpawner(
        EventProcessor eventProcessor,
        Transform npcsContainer,
        Transform monstersContainer,
        GameObject npcPlaceholder,
        GameObject monsterPlaceholder)
        : base(eventProcessor)
    {
        _npcsContainer = npcsContainer;
        _monstersContainer = monstersContainer;
        _npcPlaceHolder = npcPlaceholder;
        _monsterPlaceholder = monsterPlaceholder;
    }

    #region Spawn
    protected override void SpawnEntity(NetworkIdentity identity, NpcStatus status,
        Stats stats, Appearance appearance, bool running)
    {
        var npcgrp = NpcgrpTable.Instance.GetNpcgrp(identity.NpcId);
        var npcName = NpcNameTable.Instance.GetNpcName(identity.NpcId);

        if (npcName == null || npcgrp == null)
        {
            Debug.LogError($"Npc {identity.NpcId} could not be loaded correctly.");
            return;
        }

        identity.EntityType = npcgrp.Type;
        var npcGo = CreateNpcGameObject(npcgrp, identity);
        if (npcGo == null) return;

        var npc = InitializeNpcEntity(npcGo, identity);
        if (npc == null) return;

        ConfigureNpcComponents(npc, identity, status, stats, appearance, npcgrp, npcName, running);

        AddEntity(identity, npc);
    }

    private GameObject CreateNpcGameObject(Npcgrp npcgrp, NetworkIdentity identity)
    {
        var prefab = ModelTable.Instance.GetNpc(npcgrp.Mesh);
        if (prefab == null)
        {
            prefab = identity.EntityType == EntityType.Monster ? _monsterPlaceholder : _npcPlaceHolder;
            Debug.LogError($"Npc {identity.NpcId} could not be loaded correctly, loaded placeholder instead.");
        }

        identity.SetPosY(World.Instance.GetGroundHeight(identity.Position));
        var npcGo = GameObject.Instantiate(prefab, identity.Position, Quaternion.identity);

        npcGo.transform.eulerAngles = new Vector3(
            npcGo.transform.eulerAngles.x,
            VectorUtils.ConvertRotToUnity(identity.Heading),
            npcGo.transform.eulerAngles.z
        );

        return npcGo;
    }

    private Entity InitializeNpcEntity(GameObject npcGo, NetworkIdentity identity)
    {
        Entity npc;
        if (identity.EntityType == EntityType.NPC)
        {
            npcGo.transform.SetParent(_npcsContainer);
            npc = npcGo.GetComponent<NetworkHumanoidEntity>();
        }
        else
        {
            npcGo.transform.SetParent(_monstersContainer);
            npc = npcGo.GetComponent<NetworkMonsterEntity>();
        }
        return npc;
    }

    private void ConfigureNpcComponents(
        Entity npc,
        NetworkIdentity identity,
        Status status,
        Stats stats,
        Appearance appearance,
        Npcgrp npcgrp,
        NpcName npcName,
        bool running)
    {
        ConfigureAppearance(appearance, npcgrp);
        ConfigureIdentity(npc, identity, npcgrp, npcName);
        ConfigureStats(npc, status, stats, npcgrp);

        npc.Appearance = appearance;
        npc.Running = running;

        var npcGo = npc.gameObject;
        npcGo.transform.name = identity.Name;
        npcGo.SetActive(true);

        InitializeNpcComponents(npc);
    }

    private void ConfigureAppearance(Appearance appearance, Npcgrp npcgrp)
    {
        if (appearance.RHand == 0)
            appearance.RHand = npcgrp.Rhand;

        if (appearance.LHand == 0)
            appearance.LHand = npcgrp.Lhand;

        if (appearance.CollisionRadius == 0)
            appearance.CollisionRadius = npcgrp.CollisionRadius;

        if (appearance.CollisionHeight == 0)
            appearance.CollisionHeight = npcgrp.CollisionHeight;
    }

    private void ConfigureIdentity(Entity npc, NetworkIdentity identity, Npcgrp npcgrp, NpcName npcName)
    {
        npc.Identity = identity;
        npc.Identity.NpcClass = npcgrp.ClassName;

        if (string.IsNullOrEmpty(npc.Identity.Name))
            npc.Identity.Name = npcName.Name;

        if (string.IsNullOrEmpty(npc.Identity.Title))
        {
            npc.Identity.Title = npcName.Title;
            if (string.IsNullOrEmpty(npc.Identity.Title) && identity.EntityType == EntityType.Monster)
                npc.Identity.Title = npcName.Title;
        }

        npc.Identity.TitleColor = npcName.TitleColor;
    }

    private void ConfigureStats(Entity npc, Status status, Stats stats, Npcgrp npcgrp)
    {
        npc.Status = status;
        npc.Status.Hp = (int)npcgrp.MaxHp;
        npc.Stats = stats;
        npc.Stats.MaxHp = (int)npcgrp.MaxHp;
    }

    private void InitializeNpcComponents(Entity npc)
    {
        npc.ReferenceHolder.AnimationController.Initialize();
        npc.ReferenceHolder.Gear.Initialize(npc.Identity.Id, npc.RaceId);
        npc.Initialize();
    }

    protected override void AddEntity(NetworkIdentity identity, Entity npc)
    {
        WorldSpawner.Instance.AddNpc(identity.Id, npc);
        WorldSpawner.Instance.AddObject(identity.Id, npc);
    }
    #endregion

    #region Update
    protected override void UpdateEntity(Entity entity, NetworkIdentity identity,
        NpcStatus status, Stats stats, Appearance appearance, bool running)
    {
        Debug.LogWarning("[" + Thread.CurrentThread.ManagedThreadId + "] UPDATE ENTITY FUNC");
        UpdateNpcComponents(entity, identity, status, stats, appearance, running);
    }

    private void UpdateNpcComponents(
        Entity entity,
        NetworkIdentity identity,
        NpcStatus status,
        Stats stats,
        Appearance appearance,
        bool running)
    {
        Debug.LogWarning("[" + Thread.CurrentThread.ManagedThreadId + "] UpdateNpcComponents");
        entity.Identity.UpdateEntityPartial(identity);

        var networkTransform = ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive;
        networkTransform.SetNewPosition(identity.Position);

        // float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        // networkTransform.SetFinalRotation(rotation);

        entity.Stats.UpdateStats(stats);
        entity.Running = running;

        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
    }
    #endregion
}
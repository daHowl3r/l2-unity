using System;
using UnityEngine;

[System.Serializable]
public abstract class Combat : MonoBehaviour
{
    [SerializeField] protected EntityReferenceHolder _referenceHolder;

    [Header("Combat")]
    [SerializeField] private int _targetId;
    [SerializeField] protected Entity _target;
    [SerializeField] protected Entity _attackTarget;
    [SerializeField] private long _stopAutoAttackTime;
    [SerializeField] private long _startAutoAttackTime;

    public int TargetId { get => _targetId; set => _targetId = value; }
    public Entity Target { get { return _target; } set { _target = value; } }
    public Entity AttackTarget { get { return _attackTarget; } set { _attackTarget = value; } }
    public long StopAutoAttackTime { get { return _stopAutoAttackTime; } }
    public long StartAutoAttackTime { get { return _startAutoAttackTime; } }
    protected Status Status { get { return _referenceHolder.Entity.Status; } }
    protected BaseAnimationAudioHandler AudioHandler { get { return _referenceHolder.AudioHandler; } }
    protected BaseAnimationController AnimationController { get { return _referenceHolder.AnimationController; } }

    private void Awake()
    {
        if (_referenceHolder == null)
        {
            Debug.LogWarning($"[{transform.name}] EntityReferenceHolder was not assigned, please pre-assign it to avoid unecessary load.");
            _referenceHolder = GetComponent<EntityReferenceHolder>();
        }
    }

    public virtual void Initialize()
    {
        if (_referenceHolder == null)
        {
            Debug.LogWarning($"[{transform.name}] EntityReferenceHolder was not assigned, please pre-assign it to avoid unecessary load.");
            _referenceHolder = GetComponent<EntityReferenceHolder>();
        }
    }

    public virtual void StartAutoAttacking()
    {
        _startAutoAttackTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        if (_target != null)
        {
            _attackTarget = _target;
        }
    }

    public virtual void StopAutoAttacking()
    {
        _stopAutoAttackTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _attackTarget = null;
    }

    // Called when ApplyDamage packet is received 
    public void ApplyDamage(Hit hit)
    {
        OnHit(hit);

        if (Status.Hp <= 0)
        {
            Debug.LogWarning("Trying to apply damage to a dead entity");
            return;
        }

        Status.Hp = Mathf.Max(Status.Hp - hit.Damage, 0);
    }

    public bool IsDead()
    {
        return Status.Hp <= 0;
    }

    public virtual void OnDeath() { }

    public virtual void OnRevive() { }

    protected virtual void OnHit(Hit hit)
    {
        if (!hit.isMiss())
        {
            AudioHandler.PlayDamageSound();

            if (hit.hasSoulshot())
            {
                if (hit.isCrit())
                {
                    AudioHandler.PlayCritSound();
                }
                AudioHandler.PlaySoulshotSound();
            }
            else if (hit.isCrit())
            {
                AudioHandler.PlayCritSound();
            }
            else
            {
                AudioHandler.PlayDefenseSound();
            }
        }


        // voice_sound_weapon -> play voice based on current weapon equiped (random)
        // defense sound -> only when soulshot is not activated
        // Swish sound -> only when attack missed (attacker)
    }

    public virtual void OnStopMoving()
    {
    }
}

using System;
using UnityEngine;

[System.Serializable]
public abstract class Combat : MonoBehaviour
{
    [SerializeField] protected EntityReferenceHolder _referenceHolder;

    [Header("Combat")]
    [SerializeField] private int _targetId;
    [SerializeField] protected Transform _target;
    [SerializeField] protected Transform _attackTarget;
    [SerializeField] private long _stopAutoAttackTime;
    [SerializeField] private long _startAutoAttackTime;

    public int TargetId { get => _targetId; set => _targetId = value; }
    public Transform Target { get { return _target; } set { _target = value; } }
    public Transform AttackTarget { get { return _attackTarget; } set { _attackTarget = value; } }
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

    public virtual bool StartAutoAttacking()
    {
        if (_target == null)
        {
            Debug.LogWarning("Trying to attack a null target");
            return false;
        }

        _startAutoAttackTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _attackTarget = _target;

        return true;
    }

    public virtual bool StopAutoAttacking()
    {
        Debug.Log($"[{transform.name}] Stop autoattacking");
        if (_attackTarget == null)
        {
            return false;
        }

        _stopAutoAttackTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _attackTarget = null;
        return true;
    }

    // Called when ApplyDamage packet is received 
    public void ApplyDamage(int damage, bool criticalHit)
    {
        if (Status.Hp <= 0)
        {
            Debug.LogWarning("Trying to apply damage to a dead entity");
            return;
        }

        Status.Hp = Mathf.Max(Status.Hp - damage, 0);

        OnHit(criticalHit);

        if (Status.Hp <= 0)
        {
            OnDeath();
        }
    }

    public bool IsDead()
    {
        return Status.Hp <= 0;
    }

    /* Notify server that entity got attacked */
    public virtual void InflictAttack(AttackType attackType) { }

    protected virtual void OnDeath() { }

    protected virtual void OnHit(bool criticalHit)
    {
        // TODO: Add armor type for more hit sounds
        AudioManager.Instance.PlayHitSound(criticalHit, transform.position);
    }
}

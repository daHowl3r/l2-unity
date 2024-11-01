using UnityEngine;

public abstract class NetworkCombat : Combat
{
    [SerializeField] protected bool _attackStance;

    protected NetworkEntityReferenceHolder ReferenceHolder { get { return (NetworkEntityReferenceHolder)_referenceHolder; } }
    protected NetworkTransformReceive NetworkTransformReceive { get { return ReferenceHolder.NetworkTransformReceive; } }
    protected NetworkCharacterControllerReceive NetworkCharacterControllerReceive { get { return ReferenceHolder.NetworkCharacterControllerReceive; } }
    protected NetworkIdentity Identity { get { return _referenceHolder.Entity.Identity; } }
    public bool AttackStance { get { return _attackStance; } set { _attackStance = value; } }

    public override void OnDeath()
    {
        base.OnDeath();

        if (AnimationController != null)
        {
            AnimationController.enabled = false;
        }
        if (NetworkTransformReceive != null)
        {
            NetworkTransformReceive.enabled = false;
        }
        if (NetworkCharacterControllerReceive != null)
        {
            NetworkCharacterControllerReceive.enabled = false;
        }
    }

    public override void OnRevive()
    {
        if (AnimationController != null)
        {
            AnimationController.enabled = true;
        }
        if (NetworkTransformReceive != null)
        {
            NetworkTransformReceive.enabled = true;
        }
        if (NetworkCharacterControllerReceive != null)
        {
            NetworkCharacterControllerReceive.enabled = true;
        }
    }

    public void LookAtTarget()
    {
        if (AttackTarget != null && Status.Hp > 0)
        {
            NetworkTransformReceive.LookAt(_attackTarget.transform);
        }
    }

    public override void OnStopMoving()
    {
        if (_attackStance)
        {
            //Refresh autoattack animation
            // Debug.LogWarning($"[{transform.name}] Reached destination resuming autoattack animation");

            // if (AttackTarget != null && !AttackTarget.ReferenceHolder.Combat.IsDead())
            // {
            //     StartAutoAttacking();
            // }
            // else
            // {
            //     StopAutoAttacking();
            // }
        }
    }

    // public override void StartAttackStance()
    // {
    //     base.StartAttackStance();

    //     // LookAtTarget();

    //     // Debug.LogWarning($"[{transform.name}] StartAutoattacking");

    //     // _attackStance = true;

    //     // if (NetworkCharacterControllerReceive != null)
    //     // {
    //     //     // Should stop moving if autoattacking
    //     //     NetworkCharacterControllerReceive.SetDestination(
    //     //         transform.position,
    //     //         WorldCombat.Instance.GetRealAttackRange(_referenceHolder.Entity, AttackTarget));
    //     // }
    // }

    // public override void StopAttackStance()
    // {
    //     // base.StopAttackStance();

    //     // Debug.LogWarning($"[{transform.name}] StopAutoattacking");

    //     // _attackStance = false;
    // }

    public override void AttackOnce()
    {
        base.AttackOnce();

        LookAtTarget();
    }
}
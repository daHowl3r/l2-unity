using UnityEngine;

public abstract class NetworkCombat : Combat
{
    [SerializeField] protected bool _isAutoAttacking;

    protected NetworkEntityReferenceHolder ReferenceHolder { get { return (NetworkEntityReferenceHolder)_referenceHolder; } }
    protected NetworkTransformReceive NetworkTransformReceive { get { return ReferenceHolder.NetworkTransformReceive; } }
    protected NetworkCharacterControllerReceive NetworkCharacterControllerReceive { get { return ReferenceHolder.NetworkCharacterControllerReceive; } }
    protected NetworkIdentity Identity { get { return _referenceHolder.Entity.Identity; } }
    public bool IsAutoAttacking { get { return _isAutoAttacking; } set { _isAutoAttacking = value; } }

    public override void OnDeath()
    {
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
            NetworkTransformReceive.LookAt(_attackTarget);
        }
    }

    public override void OnStopMoving()
    {
        if (_isAutoAttacking)
        {
            //Refresh autoattack animation
            Debug.LogWarning($"[{transform.name}] Reached destination resuming autoattack animation");
            StartAutoAttacking();
        }
    }

    public override void StartAutoAttacking()
    {
        base.StartAutoAttacking();

        LookAtTarget();

        Debug.LogWarning($"[{transform.name}] StartAutoattacking");

        _isAutoAttacking = true;

        if (NetworkCharacterControllerReceive != null)
        {
            // Should stop moving if autoattacking
            NetworkCharacterControllerReceive.SetDestination(transform.position);
        }
    }

    public override void StopAutoAttacking()
    {
        base.StopAutoAttacking();

        Debug.LogWarning($"[{transform.name}] StopAutoattacking");

        _isAutoAttacking = false;
    }
}
public abstract class NetworkCombat : Combat
{
    protected NetworkEntityReferenceHolder ReferenceHolder { get { return (NetworkEntityReferenceHolder)_referenceHolder; } }
    protected NetworkTransformReceive NetworkTransformReceive { get { return ReferenceHolder.NetworkTransformReceive; } }
    protected NetworkCharacterControllerReceive NetworkCharacterControllerReceive { get { return ReferenceHolder.NetworkCharacterControllerReceive; } }
    protected NetworkIdentity Identity { get { return _referenceHolder.Entity.Identity; } }

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
}
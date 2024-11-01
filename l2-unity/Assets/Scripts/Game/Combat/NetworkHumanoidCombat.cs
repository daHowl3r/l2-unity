
// Used by NPCS and USERS
public class NetworkHumanoidCombat : NetworkCombat
{
    public HumanoidAnimationController HumanoidAnimationController { get { return (HumanoidAnimationController)_referenceHolder.AnimationController; } }
    public HumanoidGear Gear { get { return (HumanoidGear)_referenceHolder.Gear; } }

    public override void OnDeath()
    {
        base.OnDeath();
        HumanoidAnimationController.SetBool(HumanoidAnimType.death, true);
    }

    public override void OnRevive()
    {
        HumanoidAnimationController.SetBool(HumanoidAnimType.wait, true);
    }

    protected override void OnHit(Hit hit)
    {
        base.OnHit(hit);
        //AudioHandler.PlaySound(EntitySoundEvent.Dmg);
    }

    public override void StartAutoAttacking()
    {
        base.StartAutoAttacking();

        HumanoidAnimationController.SetBool(HumanoidAnimType.atk01, true);
        // LookAtTarget();
    }

    public override void StopAutoAttacking()
    {
        base.StopAutoAttacking();

        HumanoidAnimationController.SetBool(HumanoidAnimType.atk01, false);
        if (!NetworkCharacterControllerReceive.IsMoving() && !IsDead())
        {
            HumanoidAnimationController.SetBool(HumanoidAnimType.atkwait, true);
        }
    }
}
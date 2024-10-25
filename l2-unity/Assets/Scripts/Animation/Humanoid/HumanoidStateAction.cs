using System;

public class HumanoidStateAction : HumanoidStateBase
{
    public bool IsMoving()
    {
        return CharacterController.IsMoving();
    }

    public bool IsAttacking()
    {
        return AnimationController.LastAnim == HumanoidAnimType.atk01;
    }

    protected bool ShouldAtkWait()
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (now - Entity.Combat.StopAutoAttackTime < 5000)
        {
            if (Entity.Combat.AttackTarget == null)
            {
                return true;
            }
        }

        return false;
    }
}

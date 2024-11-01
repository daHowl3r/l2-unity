// Used by MONSTERS
using UnityEngine;

public class NetworkMonsterCombat : NetworkCombat
{
    public MonsterAnimationController MonsterAnimationController { get { return (MonsterAnimationController)_referenceHolder.AnimationController; } }

    public override void OnDeath()
    {
        base.OnDeath();
        Debug.LogWarning("DEAD");
        MonsterAnimationController.SetBool(MonsterAnimationEvent.death, true);
    }

    public override void OnRevive()
    {
        MonsterAnimationController.SetBool(MonsterAnimationEvent.wait, true);
    }

    protected override void OnHit(Hit hit)
    {
        base.OnHit(hit);
        //AudioHandler.PlaySound(EntitySoundEvent.Dmg);
    }

    public override void StartAutoAttacking()
    {
        base.StartAutoAttacking();

        MonsterAnimationController.SetBool(MonsterAnimationEvent.atk01, true);
        // LookAtTarget();
    }

    public override void StopAutoAttacking()
    {
        base.StopAutoAttacking();

        // if (!IsDead())
        // {
        //     MonsterAnimationController.SetBool(MonsterAnimationEvent.atkwait, true);
        // }
    }
}
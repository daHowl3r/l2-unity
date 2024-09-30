using UnityEngine;

public class MonsterStateDeath : MonsterStateBase
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        LoadComponents(animator);
        PlaySoundAtRatio(EntitySoundEvent.Death, audioHandler.DeathRatio);
        PlaySoundAtRatio(EntitySoundEvent.Fall, audioHandler.FallRatio);
        animator.SetBool("death", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}

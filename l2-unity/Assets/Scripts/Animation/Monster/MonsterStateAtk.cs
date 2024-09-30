using UnityEngine;

public class MonsterStateAtk : MonsterStateBase
{
    private float _lastNormalizedTime = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        LoadComponents(animator);

        AnimatorClipInfo[] clipInfos = animator.GetNextAnimatorClipInfo(0);
        if (clipInfos == null || clipInfos.Length == 0)
        {
            clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        }

        _networkAnimationController.UpdateAnimatorAtkSpdMultiplier(clipInfos[0].clip.length);

        PlaySoundAtRatio(EntitySoundEvent.Atk, audioHandler.AtkRatio);
        PlaySoundAtRatio(EntitySoundEvent.Swish, audioHandler.SwishRatio);
        _lastNormalizedTime = 0;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if the state has looped (re-entered)
        if ((stateInfo.normalizedTime - _lastNormalizedTime) >= 1f)
        {
            // This block will be executed once when the state is re-entered after completion
            _lastNormalizedTime = stateInfo.normalizedTime;
            PlaySoundAtRatio(EntitySoundEvent.Atk, audioHandler.AtkRatio);
            PlaySoundAtRatio(EntitySoundEvent.Swish, audioHandler.SwishRatio);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}

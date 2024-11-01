using UnityEngine;

public class MonsterStateAtk : MonsterStateAction
{
    private float _lastNormalizedTime;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        LoadComponents(animator);

        AnimatorClipInfo[] clipInfos = animator.GetNextAnimatorClipInfo(0);
        if (clipInfos == null || clipInfos.Length == 0)
        {
            clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        }

        AnimController.UpdateAnimatorAtkSpdMultiplier(clipInfos[0].clip.length);

        SetBool(MonsterAnimationEvent.wait, false);
        SetBool(MonsterAnimationEvent.atkwait, false);

        PlaySoundAtRatio(EntitySoundEvent.Atk, AudioHandler.AtkRatio);
        PlaySoundAtRatio(EntitySoundEvent.Swish, AudioHandler.SwishRatio);

        _lastNormalizedTime = 0;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (IsMoving())
        {
            SetBool(MonsterAnimationEvent.atk01, false);

            if (Entity.Running)
            {
                SetBool(MonsterAnimationEvent.run, true);
            }
            else
            {
                SetBool(MonsterAnimationEvent.walk, true);
            }

            return;
        }

        if ((stateInfo.normalizedTime - _lastNormalizedTime) >= 1f)
        {
            SetBool(MonsterAnimationEvent.atk01, false);
            _lastNormalizedTime = stateInfo.normalizedTime;
            PlaySoundAtRatio(EntitySoundEvent.Atk, AudioHandler.AtkRatio);
            PlaySoundAtRatio(EntitySoundEvent.Swish, AudioHandler.SwishRatio);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetBool(MonsterAnimationEvent.atk01, false);
    }
}

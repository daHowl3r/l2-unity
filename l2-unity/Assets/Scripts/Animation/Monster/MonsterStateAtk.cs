using System;
using UnityEngine;

public class MonsterStateAtk : MonsterStateAction
{
    private float clipLength;
    private float _lastNormalizedTime = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        LoadComponents(animator);

        AnimatorClipInfo[] clipInfos = animator.GetNextAnimatorClipInfo(0);
        if (clipInfos == null || clipInfos.Length == 0)
        {
            clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        }

        clipLength = clipInfos[0].clip.length;

        AnimController.UpdateAnimatorAtkSpdMultiplier(clipLength);

        SetBool(MonsterAnimationEvent.wait, false);
        SetBool(MonsterAnimationEvent.atkwait, false);

        PlaySoundAtRatio(EntitySoundEvent.Atk, AudioHandler.AtkRatio);
        PlaySoundAtRatio(EntitySoundEvent.Swish, AudioHandler.SwishRatio);

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 0.25f)
        {
            SetBool(MonsterAnimationEvent.atk01, false);
        }

        if (IsDead())
        {
            SetBool(MonsterAnimationEvent.atk01, false);
            SetBool(MonsterAnimationEvent.death, true);
            return;
        }

        if (IsMoving())
        {
            if (Entity.Running)
            {
                SetBool(MonsterAnimationEvent.atk01, false);
                SetBool(MonsterAnimationEvent.run, true);
            }
            else
            {
                SetBool(MonsterAnimationEvent.atk01, false);
                SetBool(MonsterAnimationEvent.walk, true);
            }

            return;
        }

        if (DidAttackTimeout())
        {
            SetBool(MonsterAnimationEvent.atk01, false);
            SetBool(MonsterAnimationEvent.atkwait, true);
        }

        if ((stateInfo.normalizedTime - _lastNormalizedTime) >= 1f)
        {
            _lastNormalizedTime = stateInfo.normalizedTime;
            PlaySoundAtRatio(EntitySoundEvent.Atk, AudioHandler.AtkRatio);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}

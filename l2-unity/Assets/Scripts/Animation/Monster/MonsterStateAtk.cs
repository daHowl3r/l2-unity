using System;
using UnityEngine;

public class MonsterStateAtk : MonsterStateAction
{
    private float _lastNormalizedTime;
    private float clipLength;

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
        SetBool(MonsterAnimationEvent.atk01, false);

        PlaySoundAtRatio(EntitySoundEvent.Atk, AudioHandler.AtkRatio);
        PlaySoundAtRatio(EntitySoundEvent.Swish, AudioHandler.SwishRatio);

        _lastNormalizedTime = 0;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (IsDead())
        {
            SetBool(MonsterAnimationEvent.death, true);
            return;
        }

        if (IsMoving())
        {
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

        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        // Check if AtkOnce was called, otherwise switch to AtkWait state
        // Add the attack duration and the last auto attack packet timestamp
        if (now > ((long)_referenceHolder.AnimationController.PAtkSpd) + _referenceHolder.Combat.CombatTimestamp)
        {
            SetBool(MonsterAnimationEvent.atkwait, true);
            SetBool(MonsterAnimationEvent.atk01, false);
            Debug.LogWarning("Should ATK WAIT! (StopAttack)");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}

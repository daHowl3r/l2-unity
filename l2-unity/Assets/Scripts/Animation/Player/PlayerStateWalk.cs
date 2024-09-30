using UnityEngine;

public class PlayerStateWalk : PlayerStateAction
{
    private float _lastNormalizedTime = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        LoadComponents(animator);

        _lastNormalizedTime = 0;

        foreach (var ratio in _audioHandler.WalkStepRatios)
        {
            _audioHandler.PlaySoundAtRatio(EntitySoundEvent.Step, ratio);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((stateInfo.normalizedTime - _lastNormalizedTime) >= 1f)
        {
            _lastNormalizedTime = stateInfo.normalizedTime;
            foreach (var ratio in _audioHandler.WalkStepRatios)
            {
                _audioHandler.PlaySoundAtRatio(EntitySoundEvent.Step, ratio);
            }
        }

        if (ShouldDie())
        {
            return;
        }

        if (ShouldAttack())
        {
            SetBool("atk01", true, true, false);
            return;
        }

        if (ShouldJump(true))
        {
            return;
        }

        if (ShouldRun())
        {
            return;
        }

        if (ShouldSit())
        {
            return;
        }

        if (ShouldAtkWait())
        {
            return;
        }

        if (ShouldIdle())
        {
            return;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetBool("walk", true, false, false);
    }
}

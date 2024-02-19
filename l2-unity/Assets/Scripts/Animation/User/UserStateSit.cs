using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStateSit : UserStateBase {
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        LoadComponents(animator);
        SetBool("sit", false);
        _audioHandler.PlaySound(CharacterSoundEvent.Sitdown);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        SetBool("sit", false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    }
}

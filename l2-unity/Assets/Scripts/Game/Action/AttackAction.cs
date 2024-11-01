using UnityEngine;
using static AttackingState;

public class AttackAction : L2Action
{
    public AttackAction() : base() { }

    // Local action
    public override void UseAction()
    {
        // If has a target and attack key pressed
        if (TargetManager.Instance.HasTarget())
        {
            Debug.LogWarning("Use attack action.");

            PlayerCombat.Instance.IsForcedAction = InputManager.Instance.Ctrl;
            PlayerStateMachine.Instance.ChangeIntention(Intention.INTENTION_ATTACK);
        }
    }
}
using UnityEngine;

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
            PlayerStateMachine.Instance.setIntention(Intention.INTENTION_ATTACK);
        }
    }
}
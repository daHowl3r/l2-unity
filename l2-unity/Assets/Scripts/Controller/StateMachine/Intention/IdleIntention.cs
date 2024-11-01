using UnityEngine;

public class IdleIntention : IntentionBase
{
    public IdleIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        // if (_stateMachine.IsInMovableState())
        // {
        //     _stateMachine.ChangeState(PlayerState.IDLE);
        // }
        // else if (!_stateMachine.WaitingForServerReply)
        // {
        //     _stateMachine.SetWaitingForServerReply(true);
        //     NetworkCharacterControllerShare.Instance.ShareMoveDirection(Vector3.zero);
        // }
        // else
        // {
        //     PlayerController.Instance.StopMoving();
        // }

        // IdleIntention is not triggerable by player, no need to ask server for permission
        _stateMachine.ChangeState(PlayerState.IDLE);
        PlayerController.Instance.StopMoving();

    }

    public override void Exit() { }
    public override void Update()
    {

    }
}
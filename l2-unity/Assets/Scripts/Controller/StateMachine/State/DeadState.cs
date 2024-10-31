public class DeadState : StateBase
{
    public DeadState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void HandleEvent(Event evt)
    {
        switch (evt)
        {
            case Event.REVIVED:
                _stateMachine.ChangeState(PlayerState.IDLE);
                break;
        }
    }
}
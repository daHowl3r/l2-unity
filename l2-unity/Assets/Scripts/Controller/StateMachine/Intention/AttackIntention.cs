using UnityEngine;
using static AttackingState;

public class AttackIntention : IntentionBase
{
    public AttackIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        Transform target = TargetManager.Instance.Target.Data.ObjectTransform;

        if (target == null)
        {
            return;
        }

        if (_stateMachine.State == PlayerState.ATTACKING)
        {
            if (TargetManager.Instance.IsAttackTargetSet())
            {
                return;
            }
            else
            {
                _stateMachine.ChangeIntention(Intention.INTENTION_FOLLOW);
                return;
            }
        }

        TargetManager.Instance.SetAttackTarget();

        Vector3 targetPos = TargetManager.Instance.AttackTarget.Data.ObjectTransform.position;

        float attackRange = WorldCombat.Instance.GetRealAttackRange(PlayerEntity.Instance, TargetManager.Instance.AttackTarget.Data.Entity);

        float distance = Vector3.Distance(PlayerEntity.Instance.transform.position, targetPos);
        Debug.Log($"target: {target} distance: {distance} range: {attackRange}");

        // Is close enough? Is player already waiting for server reply?
        if (distance <= attackRange * 0.95f && !_stateMachine.WaitingForServerReply)
        {
            PlayerController.Instance.UpdateFinalAngleToLookAt(TargetManager.Instance.AttackTarget.Data.ObjectTransform);
            _stateMachine.ChangeState(PlayerState.IDLE);
            _stateMachine.NotifyEvent(Event.READY_TO_ATTACK);
        }
        else
        {
            // Move to target with a 10% error margin
            PathFinderController.Instance.MoveTo(targetPos, attackRange * 0.95f);
        }
    }

    public override void Exit() { }
    public override void Update()
    {

    }
}
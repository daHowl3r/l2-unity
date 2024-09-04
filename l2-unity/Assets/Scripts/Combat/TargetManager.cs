using UnityEngine;

public class TargetManager : MonoBehaviour
{
    private TargetData _target = null;
    private TargetData _attackTarget = null;

    public TargetData Target { get { return _target; } }
    public TargetData AttackTarget { get { return _attackTarget; } }

    private static TargetManager _instance;
    public static TargetManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        _instance = null;
    }

    private void Start()
    {
        _target = null;
        _attackTarget = null;
    }

    public void SetTarget(ObjectData target)
    {
        if (target == null)
        {
            ClearTarget();
            return;
        }

        _target = new TargetData(target);

        PlayerEntity.Instance.TargetId = _target.Identity.Id;
        PlayerEntity.Instance.Target = _target.Data.ObjectTransform;
        GameClient.Instance.ClientPacketHandler.SendRequestSetTarget(_target.Identity.Id);
    }

    public void SetAttackTarget()
    {
        _attackTarget = _target;
    }

    public void ClearAttackTarget()
    {
        _attackTarget = null;
    }

    public bool IsAttackTargetSet()
    {
        return _target != null && _attackTarget != null && _target == _attackTarget;
    }

    public bool HasTarget()
    {
        return _target != null && _target.Data.ObjectTransform != null;
    }

    public bool HasAttackTarget()
    {
        return _attackTarget != null;
    }

    public void ClearTarget()
    {
        if (HasTarget())
        {
            if (PlayerEntity.Instance.TargetId != -1)
            {
                GameClient.Instance.ClientPacketHandler.SendRequestSetTarget(-1);
                PlayerEntity.Instance.TargetId = -1;
                PlayerEntity.Instance.Target = null;
            }

            _target = null;
            _attackTarget = null;
        }
    }

    void Update()
    {
        if (PlayerEntity.Instance == null)
        {
            return;
        }

        if (HasTarget())
        {
            _target.Distance = Vector3.Distance(
                PlayerController.Instance.transform.position,
                _target.Data.ObjectTransform.position);
        }
        else
        {
            ClearTarget();
        }
    }
}

using System;
using UnityEngine;

[RequireComponent(typeof(NetworkTransformShare)), RequireComponent(typeof(CharacterController))]
public class NetworkCharacterControllerShare : MonoBehaviour
{
    private CharacterController _characterController;
    [SerializeField] private int _sharingLoopDelayMs = 100;
    [SerializeField] private Vector3 _lastDirection;
    [SerializeField] private long _lastSharingTimestamp = 0;
    [SerializeField] private int _heading;

    public int Heading { get { return _heading; } set { _heading = value; } }

    private static NetworkCharacterControllerShare _instance;
    public static NetworkCharacterControllerShare Instance { get { return _instance; } }

    public void Awake()
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

    private void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        if (_characterController == null || World.Instance.OfflineMode)
        {
            this.enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        Vector3 newDirection = PlayerController.Instance.MoveDirection.normalized;
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (ShouldShareMoveDirection(newDirection, now))
        {
            _lastSharingTimestamp = now;

            if (VectorUtils.IsVectorZero2D(newDirection))
            {
                NetworkTransformShare.Instance.SharePosition();
                NetworkTransformShare.Instance.ShouldShareRotation = false;
            }
            else
            {
                NetworkTransformShare.Instance.ShouldShareRotation = true;
            }

            ShareMoveDirection(newDirection);
            _lastDirection = newDirection;
        }
    }

    private bool ShouldShareMoveDirection(Vector3 newDirection, long timestamp)
    {
        if (_lastDirection == newDirection)
        {
            return false;
        }

        if (VectorUtils.IsVectorZero2D(_lastDirection) && !VectorUtils.IsVectorZero2D(newDirection))
        {
            // player just moved
            return true;
        }

        if (!VectorUtils.IsVectorZero2D(_lastDirection) && VectorUtils.IsVectorZero2D(newDirection))
        {
            // player just stopped
            return true;
        }

        // Basic loop delay
        if (timestamp - _lastSharingTimestamp >= _sharingLoopDelayMs && newDirection != _lastDirection)
        {
            return true;
        }

        return false;
    }

    public void ShareMoveDirection(Vector3 moveDirection)
    {
        Debug.LogWarning(_lastDirection + " - " + moveDirection);
        if (_lastDirection.x == moveDirection.x && _lastDirection.z == moveDirection.z)
        {
            return;
        }

        if (!VectorUtils.IsVectorZero2D(moveDirection))
        {
            Heading = CalculateHeading(moveDirection);
        }

        _lastDirection = moveDirection;

        GameClient.Instance.ClientPacketHandler.UpdateMoveDirection(moveDirection, Heading);
    }

    private int CalculateHeading(Vector3 moveDirection)
    {
        float directionAngle = VectorUtils.CalculateMoveDirectionAngle(moveDirection.x, moveDirection.z);
        return (int)VectorUtils.ConvertRotToUnreal(directionAngle);
    }

    public void ForceShareMoveDirection()
    {
        ShareMoveDirection(PlayerController.Instance.MoveDirection.normalized);
    }
}

using UnityEngine;

public class RequestMoveDirectionPacket : ClientPacket
{

    public RequestMoveDirectionPacket(Vector3 direction) : base((byte)GameClientPacketType.RequestMoveDirection)
    {
        WriteD(direction.x);
        // WriteD(direction.y);
        WriteD(direction.z);
        float directionAngle = VectorUtils.CalculateMoveDirectionAngle(direction.x, direction.z);
        int heading = (int)VectorUtils.ConvertRotToUnreal(directionAngle);
        WriteI(heading);
        Debug.Log($"RequestMoveDirection: Angle:{directionAngle} Heading:{heading}");

        BuildPacket();
    }
}

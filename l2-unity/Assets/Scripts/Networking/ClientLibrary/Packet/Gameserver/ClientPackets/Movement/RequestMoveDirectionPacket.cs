using UnityEngine;

public class RequestMoveDirectionPacket : ClientPacket
{

    public RequestMoveDirectionPacket(Vector3 direction, int heading) : base((byte)GameClientPacketType.RequestMoveDirection)
    {
        WriteD(direction.x);
        WriteD(direction.z);
        WriteI(heading);

        BuildPacket();
    }
}

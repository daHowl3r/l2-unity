public class RequestActionPacket : ClientPacket
{
    public RequestActionPacket(int objectId) : base((byte)GameClientPacketType.RequestAction)
    {
        WriteI(objectId);
        BuildPacket();
    }
}

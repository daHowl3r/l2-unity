public class RequestUnsetTargetPacket : ClientPacket
{
    public RequestUnsetTargetPacket(bool cancelCast) : base((byte)GameClientPacketType.RequestUnsetTarget)
    {
        WriteB(0);
        WriteB(cancelCast ? (byte)0 : (byte)1);
        BuildPacket();
    }
}

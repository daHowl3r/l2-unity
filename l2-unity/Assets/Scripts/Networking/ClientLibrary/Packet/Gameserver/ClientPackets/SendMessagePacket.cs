using UnityEditor;

public class SendMessagePacket : ClientPacket
{
    public SendMessagePacket(string text, MessageType messageType, int pmTarget) : base((byte)GameClientPacketType.SendMessage)
    {
        WriteS(text);
        WriteI((int)messageType);

        if (messageType == MessageType.TELL)
        {
            WriteI(pmTarget);
        }

        BuildPacket();
    }
}
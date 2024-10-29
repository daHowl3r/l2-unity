public class CreatureSayPacket : ServerPacket
{
    public int ObjectId { get; private set; }
    public MessageType MessageType { get; private set; }
    public string Sender { get; private set; }
    public string Text { get; private set; }
    public int SystemStringId { get; private set; }
    public int SystemMessageId { get; private set; }

    public CreatureSayPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        ObjectId = ReadI();
        MessageType = (MessageType)ReadI();
        if (MessageType != MessageType.BOAT)
        {
            Sender = ReadS();
            Text = ReadS();
        }
        else
        {
            SystemStringId = ReadI();
            SystemMessageId = ReadI();
        }
    }
}
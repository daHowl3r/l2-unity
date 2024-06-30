public enum GameServerPacketType : byte
{
    Ping = 0x00,
    Key = 0x01,
    LoginFail = 0x02,
    CharSelectionInfo = 0x03,
    MessagePacket = 0x04,
    SystemMessage = 0x05,
    PlayerInfo = 0x06,
    ObjectPosition = 0x07,
    RemoveObject = 0x08,
    ObjectRotation = 0x09,
    ObjectAnimation = 0x0A,
    ApplyDamage = 0x0B,
    NpcInfo = 0x0C,
    ObjectMoveTo = 0x0D,
    UserInfo = 0x0E,
    ObjectMoveDirection = 0x0F,
    GameTime = 0x10,
    EntitySetTarget = 0x11,
    AutoAttackStart = 0x12,
    AutoAttackStop = 0x13,
    ActionFailed = 0x14
}

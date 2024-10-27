public enum GameServerPacketType : byte
{
    VersionCheck = 0x00,
    CharSelectionInfo = 0x13,
    LoginFail = 0x14,
    CharCreateOk = 0x19,
    CharCreateFail = 0x1a,


















    MessagePacket = 0xF4,
    SystemMessage = 0xF5,
    PlayerInfo = 0xF6,
    ObjectPosition = 0xF7,
    RemoveObject = 0xF8,
    ObjectRotation = 0xF9,
    ObjectAnimation = 0xFA,
    ApplyDamage = 0xFB,
    NpcInfo = 0xFC,
    ObjectMoveTo = 0xFD,
    UserInfo = 0xFE,
    ObjectMoveDirection = 0xFF,
    GameTime = 0xD0,
    EntitySetTarget = 0xD1,
    AutoAttackStart = 0xD2,
    AutoAttackStop = 0xD3,
    ActionFailed = 0xD4,
    ServerClose = 0xD5,
    StatusUpdate = 0xD6,
    ActionAllowed = 0xD7,
    InventoryItemList = 0xD8,
    InventoryUpdate = 0xD9,
    LeaveWorld = 0xDA,
    RestartReponse = 0xDB,
    ShortcutInit = 0xDC,
    ShortcutRegister = 0xDD,
    SocialAction = 0xDE,
    ChangeWaitType = 0xDF,
    ChangeMoveType = 0xE0,

    Ping = 0xE1,

}

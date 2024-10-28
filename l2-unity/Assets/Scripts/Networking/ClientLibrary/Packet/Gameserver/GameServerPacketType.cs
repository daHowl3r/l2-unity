public enum GameServerPacketType : int
{
    VersionCheck = 0x00,
    UserInfo = 0x04,
    CharSelectionInfo = 0x13,
    LoginFail = 0x14,
    CharSelected = 0x15,
    CharCreateOk = 0x19,
    CharCreateFail = 0x1a,
    InventoryItemList = 0x1b,
    ShortcutRegister = 0x44,
    ShortcutInit = 0x45,
    SystemMessage = 0x64,


















    MessagePacket = 0xFF4,
    ObjectPosition = 0xFF7,
    RemoveObject = 0xFF8,
    ObjectRotation = 0xFF9,
    ObjectAnimation = 0xFFA,
    ApplyDamage = 0xFFB,
    NpcInfo = 0xFFC,
    ObjectMoveTo = 0xFFD,
    ObjectMoveDirection = 0xFFF,
    GameTime = 0xFD0,
    EntitySetTarget = 0xFD1,
    AutoAttackStart = 0xFD2,
    AutoAttackStop = 0xFD3,
    ActionFailed = 0xFD4,
    ServerClose = 0xFD5,
    StatusUpdate = 0xFD6,
    ActionAllowed = 0xFD7,
    InventoryUpdate = 0xFD9,
    LeaveWorld = 0xFDA,
    RestartReponse = 0xFDB,
    SocialAction = 0xFDE,
    ChangeWaitType = 0xFDF,
    ChangeMoveType = 0xFE0,

    Ping = 0xFE1,

}

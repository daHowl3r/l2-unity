public enum GameServerPacketType : int
{
    VersionCheck = 0x00,
    ObjectMoveTo = 0x01,
    UserInfo = 0x04,
    CharSelectionInfo = 0x13,
    LoginFail = 0x14,
    CharSelected = 0x15,
    NpcInfo = 0x16,
    CharCreateOk = 0x19,
    InventoryUpdate = 0x27,
    CharCreateFail = 0x1a,
    InventoryItemList = 0x1b,
    ShortcutRegister = 0x44,
    ShortcutInit = 0x45,
    RestartReponse = 0x5F,
    ValidateLocation = 0x61,
    SystemMessage = 0x64,



    MessagePacket = 0xFF4,
    ObjectPosition = 0xFF7,
    RemoveObject = 0xFF8,
    ObjectRotation = 0xFF9,
    ObjectAnimation = 0xFFA,
    ApplyDamage = 0xFFB,
    ObjectMoveDirection = 0xFFF,
    GameTime = 0xFD0,
    EntitySetTarget = 0xFD1,
    AutoAttackStart = 0xFD2,
    AutoAttackStop = 0xFD3,
    ActionFailed = 0xFD4,
    ServerClose = 0xFD5,
    StatusUpdate = 0xFD6,
    ActionAllowed = 0xFD7,
    LeaveWorld = 0xFDA,

    SocialAction = 0xFDE,
    ChangeWaitType = 0xFDF,
    ChangeMoveType = 0xFE0,

    Ping = 0xFE1,

}

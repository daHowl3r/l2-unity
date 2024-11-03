public enum GameServerPacketType : int
{
    VersionCheck = 0x00,
    ObjectMoveTo = 0x01,
    UserInfo = 0x04,
    Attack = 0x05,
    DoDie = 0x06,
    Revive = 0x07,
    ActionAllowed = 0x08,
    StatusUpdate = 0x0e,
    NpcHtml = 0x0f,
    RemoveObject = 0x12,
    CharSelectionInfo = 0x13,
    LoginFail = 0x14,
    CharSelected = 0x15,
    NpcInfo = 0x16,
    CharCreateOk = 0x19,
    InventoryUpdate = 0x27,
    CharCreateFail = 0x1a,
    InventoryItemList = 0x1b,
    ActionFailed = 0x25,
    TeleportToLocation = 0x28,
    EntityTargetSet = 0x29,
    EntityTargetUnset = 0x2a,
    AutoAttackStart = 0x2b,
    AutoAttackStop = 0x2c,
    ChangeMoveType = 0x2e,
    ChangeWaitType = 0x2f,
    ShortcutRegister = 0x44,
    ShortcutInit = 0x45,
    StopMove = 0x47,
    CreatureSay = 0x4a,
    RestartReponse = 0x5F,
    ValidateLocation = 0x61,
    SystemMessage = 0x64,
    MyTargetSet = 0xA6,
    ExAutoSoulshot = 0xFE,



    // Deprecated / need to implement
    ObjectPosition = 0xFF7,
    ObjectRotation = 0xFF9,
    ObjectAnimation = 0xFFA,
    ObjectMoveDirection = 0xFFF,
    GameTime = 0xFD0,
    ServerClose = 0xFD5,
    LeaveWorld = 0xFDA,

    SocialAction = 0xFDE,

    Ping = 0xFE1,

}

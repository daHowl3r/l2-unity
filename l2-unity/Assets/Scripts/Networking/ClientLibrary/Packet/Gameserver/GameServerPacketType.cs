public enum GameServerPacketType : int
{
    VersionCheck = 0x00,
    ObjectMoveTo = 0x01,
    UserInfo = 0x04,
    ApplyDamage = 0x05,
    StatusUpdate = 0x0e,
    CharSelectionInfo = 0x13,
    LoginFail = 0x14,
    CharSelected = 0x15,
    NpcInfo = 0x16,
    CharCreateOk = 0x19,
    InventoryUpdate = 0x27,
    CharCreateFail = 0x1a,
    InventoryItemList = 0x1b,
    EntityTargetSet = 0x29,
    EntityTargetUnset = 0x2a,
    AutoAttackStart = 0x2b,
    AutoAttackStop = 0x2c,
    ChangeMoveType = 0x2e,
    ChangeWaitType = 0x2f,
    ShortcutRegister = 0x44,
    ShortcutInit = 0x45,
    CreatureSay = 0x4a,
    RestartReponse = 0x5F,
    ValidateLocation = 0x61,
    SystemMessage = 0x64,
    MyTargetSet = 0xA6,








    ObjectPosition = 0xFF7,
    RemoveObject = 0xFF8,
    ObjectRotation = 0xFF9,
    ObjectAnimation = 0xFFA,
    ObjectMoveDirection = 0xFFF,
    GameTime = 0xFD0,
    ActionFailed = 0xFD4,
    ServerClose = 0xFD5,
    ActionAllowed = 0xFD7,
    LeaveWorld = 0xFDA,

    SocialAction = 0xFDE,

    Ping = 0xFE1,

}

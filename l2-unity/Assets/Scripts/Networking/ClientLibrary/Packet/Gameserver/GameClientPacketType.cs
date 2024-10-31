public enum GameClientPacketType : byte
{
    ProtocolVersion = 0x00,
    RequestMoveDirection = 0x02,
    EnterWorld = 0x03,
    RequestAction = 0x04,
    RequestSetTarget = 0x05,
    AuthRequest = 0x08,
    Disconnect = 0x09,
    RequestAttack = 0x0a,
    RequestCharCreate = 0x0b,
    RequestCharSelect = 0x0d,
    RequestInventoryOpen = 0x0f,
    RequestInventoryUpdateOrder = 0x10,
    RequestUnEquip = 0x11,
    RequestDropItem = 0x12,
    UseItem = 0x14,
    RequestShortcutReg = 0x33,
    RequestShortcutDel = 0x35,
    RequestUnsetTarget = 0x37,
    SendMessage = 0x38,
    RequestActionUse = 0x45,
    RequestRestart = 0x46,
    ValidatePosition = 0x48,
    RequestDestroyItem = 0x59,
    RequestRestartPoint = 0x6d,



    Ping = 0xF0,
    RequestRotate = 0xF3,
    RequestAnim = 0xF4,
}

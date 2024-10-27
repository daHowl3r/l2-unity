public enum GameClientPacketType : byte
{
    ProtocolVersion = 0x00,
    LoadWorld = 0x03,
    AuthRequest = 0x08,
    Disconnect = 0x09,
    RequestAttack = 0x0a,
    RequestCharCreate = 0x0b,
    RequestCharSelect = 0x0d,
    RequestInventoryOpen = 0x0f,
    RequestUnEquip = 0x11,
    RequestDropItem = 0x12,
    UseItem = 0x14,
    RequestShortcutReg = 0x33,
    RequestShortcutDel = 0x35,
    RequestActionUse = 0x45,
    RequestRestart = 0x46,
    RequestDestroyItem = 0x59,



    Ping = 0xF0,
    SendMessage = 0xF1,
    RequestMove = 0xF2,
    RequestRotate = 0xF3,
    RequestAnim = 0xF4,
    RequestMoveDirection = 0xF5,
    RequestInventoryUpdateOrder = 0xF6,
    RequestAutoAttack = 0xF7,
    RequestSetTarget = 0xF8,
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestAutoAttackPacket : ClientPacket {
    public RequestAutoAttackPacket() : base((byte)ClientPacketType.RequestAutoAttack) {
        BuildPacket();
    }
}

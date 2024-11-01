using System;
using UnityEngine;

public class MyTargetSetPacket : ServerPacket
{
    public int TargetId { get; private set; }
    public int LevelGap { get; private set; }

    public MyTargetSetPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        TargetId = ReadI();
        LevelGap = ReadH();
    }
}

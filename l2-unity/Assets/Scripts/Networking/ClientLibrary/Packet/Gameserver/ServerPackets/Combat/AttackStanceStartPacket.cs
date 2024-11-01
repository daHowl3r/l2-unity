using System;
using UnityEngine;

public class AttackStanceStartPacket : ServerPacket
{
    public int EntityId { get; private set; }

    public AttackStanceStartPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            EntityId = ReadI();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}

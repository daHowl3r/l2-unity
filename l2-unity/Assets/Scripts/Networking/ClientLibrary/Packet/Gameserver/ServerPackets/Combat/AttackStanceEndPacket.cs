using System;
using UnityEngine;

public class AttackStanceEndPacket : ServerPacket
{
    public int EntityId { get; private set; }

    public AttackStanceEndPacket(byte[] d) : base(d)
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

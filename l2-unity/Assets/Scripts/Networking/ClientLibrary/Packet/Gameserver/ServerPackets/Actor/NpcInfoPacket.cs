using System;
using UnityEngine;

public class NpcInfoPacket : ServerPacket
{
    public NetworkIdentity Identity { get; private set; }
    public NpcStatus Status { get; private set; }
    public Stats Stats { get; private set; }
    public Appearance Appearance { get; private set; }
    public bool Running { get; set; }

    public NpcInfoPacket(byte[] d) : base(d)
    {
        Identity = new NetworkIdentity();
        Status = new NpcStatus();
        Appearance = new Appearance();
        Stats = new Stats();
        Parse();
    }

    public override void Parse()
    {
        try
        {
            Identity.Id = ReadI();
            Identity.NpcId = ReadI() - 1000000;
            ReadI(); // is attackable

            Identity.SetPosZ(ReadI() / 52.5f);
            Identity.SetPosX(ReadI() / 52.5f);
            Identity.SetPosY(ReadI() / 52.5f);
            Identity.Heading = ReadI();

            ReadI();

            // Stats
            Stats.MAtkSpd = ReadI();
            Stats.PAtkSpd = ReadI();
            Stats.RunSpeed = ReadI();
            Stats.WalkSpeed = ReadI();
            Stats.RunSpeed = ReadI();
            Stats.WalkSpeed = ReadI();
            Stats.RunSpeed = ReadI();
            Stats.WalkSpeed = ReadI();
            Stats.RunSpeed = ReadI();
            Stats.WalkSpeed = ReadI();

            Stats.MoveSpeedMultiplier = (float)ReadD();
            Stats.AttackSpeedMultiplier = (float)ReadD();

            Appearance.CollisionRadius = (float)ReadD() / 52.5f;
            Appearance.CollisionHeight = (float)ReadD() / 52.5f;

            Appearance.RHand = ReadI(); //rhand
            ReadI(); //chest
            Appearance.LHand = ReadI(); //lhand

            ReadB();
            Running = ReadB() == 1;
            ReadB(); //in combat -> Better handled client side
            ReadB(); //dead
            ReadB(); //summoned? always 2

            Identity.Name = ReadS();
            Identity.Title = ReadS();

            ReadI();
            ReadI();
            ReadI();

            ReadI(); //abnormal effect

            ReadI(); //clan
            ReadI(); //clancrest
            ReadI(); //ally
            ReadI(); //allycrest

            ReadB(); //movetype
            ReadB();

            Appearance.CollisionRadius = (float)ReadD() / 52.5f;
            Appearance.CollisionHeight = (float)ReadD() / 52.5f;

            ReadI(); //enchant effct
            ReadI(); //flying

            Stats.RunSpeed = (int)(Stats.MoveSpeedMultiplier > 0 ? Stats.RunSpeed * Stats.MoveSpeedMultiplier : Stats.RunSpeed);
            Stats.WalkSpeed = (int)(Stats.MoveSpeedMultiplier > 0 ? Stats.WalkSpeed * Stats.MoveSpeedMultiplier : Stats.WalkSpeed);

            Stats.AttackRange = ReadI() / 52.5f;

            // Debug.LogWarning(ToString());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public override string ToString()
    {
        return $"Identity: {{ " +
               $"Id = {Identity.Id}, " +
               $"NpcId = {Identity.NpcId}, " +
               $"Position = {Identity.Position}), " +
               $"Heading = {Identity.Heading}, " +
               $"Name = \"{Identity.Name}\", " +
               $"Title = \"{Identity.Title}\" }}\n" +

               $"Stats: {{ " +
               $"MAtkSpd = {Stats.MAtkSpd}, " +
               $"PAtkSpd = {Stats.PAtkSpd}, " +
               $"RunSpeed = {Stats.RunSpeed}, " +
               $"WalkSpeed = {Stats.WalkSpeed}, " +
               $"MoveSpeedMultiplier = {Stats.MoveSpeedMultiplier:F2}, " +
               $"AttackSpeedMultiplier = {Stats.AttackRange:F2}, " +
               $"AttackSpeedMultiplier = {Stats.AttackSpeedMultiplier:F2} }}\n" +

               $"Appearance: {{ " +
               $"CollisionRadius = {Appearance.CollisionRadius:F2}, " +
               $"CollisionHeight = {Appearance.CollisionHeight:F2}, " +
               $"RHand = {Appearance.RHand}, " +
               $"LHand = {Appearance.LHand} }}\n" +

               $"Other: {{ " +
               $"Running = {Running} }}";
    }
}

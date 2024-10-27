using System.Collections.Generic;
using UnityEngine;

public class CharSelectionInfoPacket : ServerPacket
{
    private int _charCount;
    private int _maximumSlots;
    private List<CharSelectionInfoPackage> _characters;
    private int _selectedSlotId;
    private int _sessionId;

    public int CharCount { get { return _charCount; } }
    public int MaximumSlots { get { return _maximumSlots; } }
    public List<CharSelectionInfoPackage> Characters { get { return _characters; } }
    public int SelectedSlotId { get { return _selectedSlotId; } }
    public int SessionId { get { return _sessionId; } }

    public CharSelectionInfoPacket(byte[] d) : base(d)
    {
        _characters = new List<CharSelectionInfoPackage>();

        Parse();
    }

    public override void Parse()
    {
        _charCount = ReadI();
        _maximumSlots = 9;

        for (int i = 0; i < _charCount; i++)
        {
            CharSelectionInfoPackage character = new CharSelectionInfoPackage();
            PlayerAppearance appearance = new PlayerAppearance();
            PlayerStatus status = new PlayerStatus();
            PlayerStats stats = new PlayerStats();

            character.Slot = i;
            character.Name = ReadS();
            character.Id = ReadI();
            character.Account = ReadS();
            _sessionId = ReadI();
            character.ClanId = ReadI();

            ReadI();

            appearance.Sex = (byte)ReadI();
            appearance.Race = (byte)ReadI();
            character.BaseClassId = (byte)ReadI();
            //character.IsMage = ReadB() == 1;

            ReadI();

            float y = ReadI() / 52.5f;
            float z = ReadI() / 52.5f;
            float x = ReadI() / 52.5f;

            character.Position = new Vector3(x, y, z);

            status.Hp = (int)ReadD();
            status.Mp = (int)ReadD();

            character.Sp = ReadI();
            character.Exp = (int)ReadL();
            stats.Level = ReadI();
            // character.ExpPercent = ReadF();

            character.Karma = ReadI();
            character.PkKills = ReadI();
            character.PvpKills = ReadI();

            ReadI();
            ReadI();
            ReadI();
            ReadI();
            ReadI();
            ReadI();
            ReadI();

            ReadI(); //HairAll?
            ReadI(); //Rear
            ReadI(); //Lear
            ReadI(); //Neck
            ReadI(); //Rfinger
            ReadI(); //Lfinger
            ReadI(); //Head
            appearance.RHand = ReadI();
            appearance.LHand = ReadI();
            appearance.Gloves = ReadI();
            appearance.Chest = ReadI();
            appearance.Legs = ReadI();
            appearance.Feet = ReadI();
            ReadI(); //Cloak
            appearance.RHand = ReadI();
            ReadI(); //Hair
            ReadI(); //Face

            ReadI(); //HairAll?
            ReadI(); //Rear
            ReadI(); //Lear
            ReadI(); //Neck
            ReadI(); //Rfinger
            ReadI(); //Lfinger
            ReadI(); //Head
            appearance.RHand = ReadI();
            appearance.LHand = ReadI();
            appearance.Gloves = ReadI();
            appearance.Chest = ReadI();
            appearance.Legs = ReadI();
            appearance.Feet = ReadI();
            ReadI(); //Cloak
            appearance.RHand = ReadI();
            ReadI(); //Hair
            ReadI(); //Face

            appearance.HairStyle = (byte)ReadI();
            appearance.HairColor = (byte)ReadI();
            appearance.Face = (byte)ReadI();

            stats.MaxHp = (int)ReadD();
            stats.MaxMp = (int)ReadD();

            character.DeleteTimer = ReadI();
            character.ClassId = (byte)ReadI();
            character.Selected = ReadI() == 1;

            ReadB(); // enchant effect
            ReadI(); // augmentation id

            if (character.Selected)
            {
                _selectedSlotId = i;
            }

            character.IsMage = CharacterClassParser.IsMage((CharacterClass)character.ClassId);

            character.CharacterRaceAnimation = CharacterModelTypeParser.ParseRace((CharacterRace)appearance.Race, appearance.Sex, character.IsMage);

            character.PlayerAppearance = appearance;
            character.PlayerStatus = status;
            character.PlayerStats = stats;

            _characters.Add(character);
        }
    }
}


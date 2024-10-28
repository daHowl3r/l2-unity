using UnityEngine;

public class UserGear : HumanoidGear
{
    [Header("References")]
    [SerializeField] private SkinnedMeshSync _skinnedMeshSync;
    [SerializeField] private GameObject _bodypartsContainer;

    [Header("Armors")]
    [Header("Meta")]
    [SerializeField] private Armor _torsoMeta;
    [SerializeField] private Armor _fullarmorMeta;
    [SerializeField] private Armor _legsMeta;
    [SerializeField] private Armor _glovesMeta;
    [SerializeField] private Armor _bootsMeta;

    [Header("Models")]
    [SerializeField] private GameObject _torso;
    [SerializeField] private GameObject _fullarmor;
    [SerializeField] private GameObject _legs;
    [SerializeField] private GameObject _gloves;
    [SerializeField] private GameObject _boots;


    public override void Initialize(int ownderId, CharacterModelType raceId)
    {
        base.Initialize(ownderId, raceId);

        if (_bodypartsContainer == null)
        {
            Debug.LogWarning($"[{transform.name}] bodypartsContainer was not assigned, please pre-assign it to avoid unecessary load.");
            _bodypartsContainer = transform.GetChild(0).GetChild(1).gameObject;
        }

        if (_skinnedMeshSync == null)
        {
            Debug.LogWarning($"[{transform.name}] SkinnedMeshSync was not assigned, please pre-assign it to avoid unecessary load.");
            _skinnedMeshSync = _bodypartsContainer.GetComponentInChildren<SkinnedMeshSync>();
        }
    }

    protected override Transform GetLeftHandBone()
    {
        if (_leftHandBone == null)
        {
            Debug.LogWarning($"[{transform.name}] Left hand bone was not assigned, please pre-assign it to avoid unecessary load.");
            _leftHandBone = transform.FindRecursive("Weapon_L_Bone");
        }
        return _leftHandBone;
    }

    protected override Transform GetRightHandBone()
    {
        if (_rightHandBone == null)
        {
            Debug.LogWarning($"[{transform.name}] Right hand bone was not assigned, please pre-assign it to avoid unecessary load.");
            _rightHandBone = transform.FindRecursive("Weapon_R_Bone");
        }
        return _rightHandBone;
    }

    protected override Transform GetShieldBone()
    {
        if (_shieldBone == null)
        {
            Debug.LogWarning($"[{transform.name}] Shield bone was not assigned, please pre-assign it to avoid unecessary load.");
            _shieldBone = transform.FindRecursive("Shield_L_Bone");
        }
        return _shieldBone;
    }

    public bool IsArmorAlreadyEquipped(int itemId, ItemSlot slot)
    {
        //Debug.Log($"IsArmorAlreadyEquipped ({itemId},{slot})");

        switch (slot)
        {
            case ItemSlot.SLOT_CHEST:
                return itemId == _torsoMeta.Id;
            case ItemSlot.SLOT_FULL_ARMOR:
                return itemId == _fullarmorMeta.Id;
            case ItemSlot.SLOT_LEGS:
                return itemId == _legsMeta.Id;
            case ItemSlot.SLOT_GLOVES:
                return itemId == _glovesMeta.Id;
            case ItemSlot.SLOT_FEET:
                return itemId == _bootsMeta.Id;
        }

        return true;
    }

    public override void EquipAllArmors(Appearance apr)
    {
        PlayerAppearance appearance = (PlayerAppearance)apr;
        if (appearance.Chest != 0)
        {
            EquipArmor(appearance.Chest, ItemSlot.SLOT_CHEST);
        }
        else
        {
            EquipArmor(ItemTable.NAKED_CHEST, ItemSlot.SLOT_CHEST);
        }

        if (appearance.Legs != 0)
        {
            EquipArmor(appearance.Legs, ItemSlot.SLOT_LEGS);
        }
        else
        {
            EquipArmor(ItemTable.NAKED_LEGS, ItemSlot.SLOT_LEGS);
        }

        if (appearance.Gloves != 0)
        {
            EquipArmor(appearance.Gloves, ItemSlot.SLOT_GLOVES);
        }
        else
        {
            EquipArmor(ItemTable.NAKED_GLOVES, ItemSlot.SLOT_GLOVES);
        }

        if (appearance.Feet != 0)
        {
            EquipArmor(appearance.Feet, ItemSlot.SLOT_FEET);
        }
        else
        {
            EquipArmor(ItemTable.NAKED_BOOTS, ItemSlot.SLOT_FEET);
        }
    }

    public void EquipArmor(int itemId, ItemSlot slot)
    {
        if (IsArmorAlreadyEquipped(itemId, slot))
        {
            Debug.Log($"Item {itemId} is already equipped in slot {slot}.");
            return;
        }

        Armor armor = ItemTable.Instance.GetArmor(itemId);
        if (armor == null)
        {
            Debug.LogWarning($"Can't find armor {itemId} in ItemTable");
            return;
        }

        ModelTable.L2ArmorPiece armorPiece = ModelTable.Instance.GetArmorPiece(armor, _raceId);
        if (armorPiece == null)
        {
            Debug.LogWarning($"Can't find armor {itemId} for race {_raceId} in slot {slot} in ModelTable");
            return;
        }

        GameObject mesh = Instantiate(armorPiece.baseArmorModel);
        mesh.GetComponentInChildren<SkinnedMeshRenderer>().material = armorPiece.material;

        SetArmorPiece(armor, mesh, slot);
    }

    private void SetArmorPiece(Armor armor, GameObject armorPiece, ItemSlot slot)
    {
        switch (slot)
        {
            case ItemSlot.SLOT_CHEST:
                if (_torso != null)
                {
                    DestroyImmediate(_torso);
                    _torsoMeta = null;
                }
                if (_fullarmor != null)
                {
                    DestroyImmediate(_fullarmor);
                    _fullarmorMeta = null;
                    EquipArmor(ItemTable.NAKED_LEGS, ItemSlot.SLOT_LEGS);
                }
                _torso = armorPiece;
                _torsoMeta = armor;
                _torso.transform.SetParent(_bodypartsContainer.transform, false);
                break;
            case ItemSlot.SLOT_FULL_ARMOR:
                if (_torso != null)
                {
                    DestroyImmediate(_torso);
                    _torsoMeta = null;
                }
                if (_legs != null)
                {
                    DestroyImmediate(_legs);
                    _legsMeta = null;
                }
                _fullarmor = armorPiece;
                _fullarmorMeta = armor;
                _fullarmor.transform.SetParent(_bodypartsContainer.transform, false);
                break;
            case ItemSlot.SLOT_LEGS:
                if (_legs != null)
                {
                    DestroyImmediate(_legs);
                    _legsMeta = null;
                }
                if (_fullarmor != null)
                {
                    DestroyImmediate(_fullarmor);
                    _fullarmorMeta = null;
                    EquipArmor(ItemTable.NAKED_CHEST, ItemSlot.SLOT_CHEST);
                }
                _legs = armorPiece;
                _legs.transform.SetParent(_bodypartsContainer.transform, false);
                _legsMeta = armor;
                break;
            case ItemSlot.SLOT_GLOVES:
                if (_gloves != null)
                {
                    DestroyImmediate(_gloves);
                    _glovesMeta = null;
                }
                _gloves = armorPiece;
                _gloves.transform.SetParent(_bodypartsContainer.transform, false);
                _glovesMeta = armor;
                break;
            case ItemSlot.SLOT_FEET:
                if (_boots != null)
                {
                    DestroyImmediate(_boots);
                    _bootsMeta = null;
                }
                _boots = armorPiece;
                _boots.transform.SetParent(_bodypartsContainer.transform, false);
                _bootsMeta = armor;
                break;
        }

        _skinnedMeshSync.SyncMesh();
    }
}
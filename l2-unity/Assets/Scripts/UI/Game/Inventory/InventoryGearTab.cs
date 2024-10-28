using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class InventoryGearTab : L2Tab
{
    private Dictionary<ItemSlot, GearSlot> _gearSlots;
    private Dictionary<ItemSlot, VisualElement> _gearAnchors;
    [SerializeField] private int _selectedSlot = -1;

    public override void Initialize(VisualElement chatWindowEle, VisualElement tabContainer, VisualElement tabHeader)
    {
        base.Initialize(chatWindowEle, tabContainer, tabHeader);

        _selectedSlot = -1;

        _gearAnchors?.Clear();

        _gearAnchors = new Dictionary<ItemSlot, VisualElement>
        {
            { ItemSlot.SLOT_HEAD, _windowEle.Q<VisualElement>("Helmet") },
            { ItemSlot.SLOT_GLOVES, _windowEle.Q<VisualElement>("Gloves") },
            { ItemSlot.SLOT_CHEST, _windowEle.Q<VisualElement>("Torso") },
            { ItemSlot.SLOT_FEET, _windowEle.Q<VisualElement>("Boots") },
            { ItemSlot.SLOT_LEGS, _windowEle.Q<VisualElement>("Legs") },
            { ItemSlot.SLOT_R_HAND, _windowEle.Q<VisualElement>("Rhand") },
            { ItemSlot.SLOT_L_HAND, _windowEle.Q<VisualElement>("Lhand") },
            { ItemSlot.SLOT_NECK, _windowEle.Q<VisualElement>("Neck") },
            { ItemSlot.SLOT_R_EAR, _windowEle.Q<VisualElement>("Rear") },
            { ItemSlot.SLOT_L_EAR, _windowEle.Q<VisualElement>("Lear") },
            { ItemSlot.SLOT_R_FINGER, _windowEle.Q<VisualElement>("Rring") },
            { ItemSlot.SLOT_L_FINGER, _windowEle.Q<VisualElement>("Lring") }
        };
    }

    public void UpdateItemList(List<ItemInstance> items)
    {
        //Debug.Log("Update gear slots");

        // Clean up slot callbacks and manipulators
        if (_gearSlots != null)
        {
            foreach (KeyValuePair<ItemSlot, GearSlot> kvp in _gearSlots)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.UnregisterClickableCallback();
                    kvp.Value.ClearManipulators();
                }
            }
            _gearSlots.Clear();
        }

        _gearSlots = new Dictionary<ItemSlot, GearSlot>();
        // Clean up gear anchors from any child visual element
        foreach (KeyValuePair<ItemSlot, VisualElement> kvp in _gearAnchors)
        {
            if (kvp.Value == null)
            {
                Debug.LogWarning($"Inventory gear slot {kvp.Key} is null.");
                continue;
            }

            // Clear gear slots
            kvp.Value.Clear();

            // Create gear slots
            VisualElement slotElement = InventoryWindow.Instance.InventorySlotTemplate.Instantiate()[0];
            kvp.Value.Add(slotElement);

            GearSlot slot = new GearSlot((int)kvp.Key, slotElement, this, L2Slot.SlotType.Gear);
            _gearSlots.Add(kvp.Key, slot);
        }

        items.ForEach(item =>
        {
            if (item.Equipped)
            {
                //Debug.Log("Equip item: " + item);
                if (item.BodyPart == ItemSlot.SLOT_LR_HAND)
                {
                    _gearSlots[ItemSlot.SLOT_L_HAND].AssignItem(item);
                    _gearSlots[ItemSlot.SLOT_R_HAND].AssignItem(item);
                }
                else if (item.BodyPart == ItemSlot.SLOT_FULL_ARMOR)
                {
                    _gearSlots[ItemSlot.SLOT_CHEST].AssignItem(item);
                    _gearSlots[ItemSlot.SLOT_LEGS].AssignItem(item);
                }
                else
                {
                    ItemSlot slot = (ItemSlot)item.BodyPart;
                    if (slot != ItemSlot.SLOT_NONE)
                    {
                        _gearSlots[(ItemSlot)item.BodyPart].AssignItem(item);
                    }
                    else
                    {
                        Debug.LogError("Can't equip item, assigned slot is " + slot);
                    }
                }
            }
        });

        if (_selectedSlot != -1)
        {
            SelectSlot(_selectedSlot);
        }
    }

    public override void SelectSlot(int slotPosition)
    {
        if (_selectedSlot != -1)
        {
            _gearSlots[(ItemSlot)_selectedSlot].UnSelect();
        }
        _gearSlots[(ItemSlot)slotPosition].SetSelected();
        _selectedSlot = slotPosition;
    }
}

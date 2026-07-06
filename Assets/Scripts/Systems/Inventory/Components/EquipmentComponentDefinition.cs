

using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Equiment", menuName = "Inventory/Equipment Component")]
public class EquipmentComponentDefinition :  DurabilityComponentDefinition
{
    public EquipmentType equipmentType;
    
    public Sprite icon;
    
    public Stats stats;
    
    public override ItemComponent GenerateComponents(ItemData itemData)
    {
        return new EquipmentItemComponent
        {
            definitionID = ID,
            durability = RandomDurability(),
            maxDurability = maxDurability,
            myItemData = itemData
        };
    }
}

[Flags]
public enum EquipmentType
{
    None=0,
    Accessory = 1,
    Helmet = 2,
    Chest = 4,
    Backpack = 8,
}
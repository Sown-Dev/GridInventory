

using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Equiment", menuName = "Inventory/Equipment Component")]
public class EquipmentComponentDefinition : ComponentDefinition
{
    public EquipmentType equipmentType;
    public int maxDurability;
    
    public Sprite icon;
    
    public override ItemComponent GenerateComponent()
    {
        return new EquipmentItemComponent
        {
            definitionID = ID,
            durability = maxDurability
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
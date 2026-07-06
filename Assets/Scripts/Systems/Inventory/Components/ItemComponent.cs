using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ItemComponent
{
    public int definitionID;
    
    public ItemData myItemData;
    
    public ComponentDefinition GetDefinition()
    {
        return Registry.instance.ByComponentID(definitionID);
    }
    public T GetDefinition<T>() where T : ComponentDefinition
    {
        return (T)Registry.instance.ByComponentID(definitionID);
    }
    
    public ItemComponent()
    {
    }
    
    public ItemComponent( int definitionID, ItemData myItemData)
    {
        this.definitionID = definitionID;
        this.myItemData = myItemData;
    }
    
}
[Serializable]

public class EquipmentItemComponent : DurabilityItemComponent
{
}

[Serializable]
public class DurabilityItemComponent : ItemComponent
{
    public int durability;
    public int maxDurability;
}

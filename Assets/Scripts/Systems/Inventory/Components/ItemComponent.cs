using System;
using System.Linq;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
[GeneratePropertyBag]
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
[GeneratePropertyBag]

public class EquipmentItemComponent : DurabilityItemComponent
{
}

[GeneratePropertyBag]
[Serializable]
public class DurabilityItemComponent : ItemComponent
{
    public int durability;
    public int maxDurability;
}

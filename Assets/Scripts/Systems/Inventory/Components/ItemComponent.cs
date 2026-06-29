using System;

[Serializable]
public class ItemComponent
{
    public int definitionID;
    
    public ComponentDefinition GetDefinition()
    {
        return Registry.instance.ByComponentID(definitionID);
    }
}
[Serializable]

public class EquipmentItemComponent : ItemComponent
{
    public int durability;
}
[Serializable]
public class WeaponItemComponent : ItemComponent
{
    public int definitionID;
    public int loadedAmmoCount;
    public int loadedAmmoID;
}

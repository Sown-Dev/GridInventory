using System;
using System.Linq;
using Unity.VisualScripting;

[Serializable]
public class ItemComponent
{
    public int definitionID;
    
    public ComponentDefinition GetDefinition()
    {
        return Registry.instance.ByComponentID(definitionID);
    }
    public T GetDefinition<T>() where T : ComponentDefinition
    {
        return (T)Registry.instance.ByComponentID(definitionID);
    }
}
[Serializable]

public class EquipmentItemComponent : DurabilityItemComponent
{
}
[Serializable]
public class GunItemComponent : DurabilityItemComponent
{
    public InventorySlot ammoSlot;

    public int AmmoCount()
    {
        //TODO: proper ammo slot max size;
        ammoSlot.maxStackSize = ((WeaponComponentDefinition) GetDefinition()).MagSize;
        
        if (!ammoSlot.IsEmpty())
        {
            if (true)//GetDefinition<WeaponComponentDefinition>().CompatibleAmmo.Any(x => x == ammoSlot.myItem.itemID))
            {
                 return ammoSlot?.myItem?.amount ??0;
            }
        }

        return 0;
    }

    public int MagSize()
    {
        return ((WeaponComponentDefinition) GetDefinition()).MagSize;
    }
    public int baseDamage;
    public float fireRate;
}

[Serializable]
public class DurabilityItemComponent : ItemComponent
{
    public int durability;
    public int maxDurability;
}

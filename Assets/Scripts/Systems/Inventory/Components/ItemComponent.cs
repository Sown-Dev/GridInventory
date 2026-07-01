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
        WeaponComponentDefinition definition = GetDefinition<WeaponComponentDefinition>();
        if (definition != null)
        {
            ammoSlot.maxStackSize = definition.MagSize;
        }

        if (!ammoSlot.IsEmpty())
        {
            if (definition == null || IsCompatibleAmmo(ammoSlot.myItem.itemID))
            {
                 return ammoSlot?.myItem?.amount ??0;
            }
        }

        return 0;
    }

    public bool CanAcceptAmmo(ItemData ammo)
    {
        WeaponComponentDefinition definition = GetDefinition<WeaponComponentDefinition>();
        if (definition == null || ammo == null)
        {
            return false;
        }

        ammoSlot.maxStackSize = definition.MagSize;

        if (!IsCompatibleAmmo(ammo.itemID))
        {
            return false;
        }

        if (ammoSlot.myItem == null)
        {
            return ammo.amount <= ammoSlot.maxStackSize;
        }

        if (ammoSlot.myItem.itemID != ammo.itemID)
        {
            return false;
        }

        return ammoSlot.myItem.amount + ammo.amount <= ammoSlot.maxStackSize;
    }

    public bool TryInsertAmmo(ItemData ammo)
    {
        if (!CanAcceptAmmo(ammo))
        {
            return false;
        }

        if (ammoSlot.myItem == null)
        {
            return ammoSlot.Insert(ammo);
        }

        ammoSlot.myItem.amount += ammo.amount;
        ammoSlot.OnChanged?.Invoke();
        return true;
    }

    private bool IsCompatibleAmmo(int ammoItemID)
    {
        WeaponComponentDefinition definition = GetDefinition<WeaponComponentDefinition>();
        if (definition == null || definition.CompatibleAmmo == null || definition.CompatibleAmmo.Length == 0)
        {
            return false;
        }

        return Array.IndexOf(definition.CompatibleAmmo, ammoItemID) >= 0;
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

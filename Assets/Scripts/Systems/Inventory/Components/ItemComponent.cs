using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

    public bool UseAmmo(bool simulate = false)
    {
        if (ammoSlot.IsEmpty())
            return false;
        
        if(ammoSlot.myItem.amount <= 0)
            return false;

        if (!simulate)
        {
            ammoSlot.myItem.amount--;
            ammoSlot.OnChanged?.Invoke();
        }
        return true;
        
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

        int currentAmount = ammoSlot.myItem != null && ammoSlot.myItem.itemID == ammo.itemID
            ? ammoSlot.myItem.amount
            : 0;

        return currentAmount < ammoSlot.maxStackSize;
    }

    public bool TryInsertAmmo(ItemData ammo)
    {
        if (!CanAcceptAmmo(ammo))
        {
            return false;
        }

        int currentAmount = ammoSlot.myItem != null && ammoSlot.myItem.itemID == ammo.itemID
            ? ammoSlot.myItem.amount
            : 0;

        int ammoSpace = ammoSlot.maxStackSize - currentAmount;
        int amountToInsert = Mathf.Min(ammoSpace, ammo.amount);
        if (amountToInsert <= 0)
        {
            return false;
        }

        if (ammoSlot.myItem == null)
        {
            if (amountToInsert == ammo.amount)
            {
                return ammoSlot.Insert(ammo);
            }

            ItemData acceptedAmmo = new ItemData
            {
                itemID = ammo.itemID,
                sizeX = ammo.sizeX,
                sizeY = ammo.sizeY,
                rotated = ammo.rotated,
                amount = amountToInsert,
                value = ammo.value,
                Components = ammo.Components
            };

            ammoSlot.Insert(acceptedAmmo);
            ammo.amount -= amountToInsert;
            return true;
        }

        ammoSlot.myItem.amount += amountToInsert;
        ammo.amount -= amountToInsert;
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

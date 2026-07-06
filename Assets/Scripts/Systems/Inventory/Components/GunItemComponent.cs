using System;
using Unity.Properties;
using UnityEngine;

[Serializable]
[GeneratePropertyBag]
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
            myItemData.OnChanged?.Invoke();
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
        myItemData.OnChanged?.Invoke();
        
        return true;
    }
    
    public bool IsAmmoFull()
    {
        return !ammoSlot.IsEmpty() && ammoSlot.myItem.amount >= ammoSlot.maxStackSize;
    }

// Decides which ammo type a reload would pull, without touching anything.
// Empty mag -> first compatible type the player is carrying.
// Partial mag -> must match what's already loaded.
    public bool TryGetReloadAmmoID(Inventory playerInventory, WeaponComponentDefinition def, out int ammoItemID)
    {
        ammoItemID = -1;
        if (IsAmmoFull()) return false;

        if (!ammoSlot.IsEmpty())
        {
            ammoItemID = ammoSlot.myItem.itemID;
            return playerInventory.CountItem(ammoItemID) > 0;
        }

        if (def.CompatibleAmmo == null) return false;

        foreach (int candidateID in def.CompatibleAmmo)
        {
            if (playerInventory.CountItem(candidateID) > 0)
            {
                ammoItemID = candidateID;
                return true;
            }
        }

        return false;
    }

// Pulls up to a full magazine of ammoItemID out of the player's inventory into ammoSlot.
// Only called once the reload timer has actually elapsed.
    public void FinishReload(Inventory playerInventory, int ammoItemID)
    {
        int currentAmount = ammoSlot.IsEmpty() ? 0 : ammoSlot.myItem.amount;
        int amountNeeded = ammoSlot.maxStackSize - currentAmount;
        if (amountNeeded <= 0) return;

        int consumed = playerInventory.ConsumeItem(ammoItemID, amountNeeded);
        if (consumed <= 0) return;

        if (ammoSlot.IsEmpty())
        {
            ItemDefinition ammoDef = Registry.instance != null ? Registry.instance.ByID(ammoItemID) : null;
            if (ammoDef == null) return;

            ItemData newStack = ammoDef.GenerateData();
            newStack.amount = consumed;
            ammoSlot.myItem = newStack;
        }
        else
        {
            ammoSlot.myItem.amount += consumed;
        }

        ammoSlot.OnChanged?.Invoke();
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

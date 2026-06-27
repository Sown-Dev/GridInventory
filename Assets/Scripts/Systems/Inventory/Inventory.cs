using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    public List<ItemData> inv = new List<ItemData>();
    public int sizeX;
    public int sizeY;

    public bool CanPlace(ItemData item)
    {
        int w = item.rotated ? item.sizeY : item.sizeX;
        int h = item.rotated ? item.sizeX : item.sizeY;

        if (item.posX < 0 || item.posY < 0 || item.posX + w > sizeX || item.posY + h > sizeY)
            return false;

        foreach (ItemData other in inv)
        {
            int ow = other.rotated ? other.sizeY : other.sizeX;
            int oh = other.rotated ? other.sizeX : other.sizeY;

            bool overlapX = item.posX < other.posX + ow && item.posX + w > other.posX;
            bool overlapY = item.posY < other.posY + oh && item.posY + h > other.posY;

            if (overlapX && overlapY)
                return false;
        }

        return true;
    }

    public bool PlaceItem(ItemData item)
    {
        if (!CanPlace(item)) return false;
        inv.Add(item);
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        return inv.Remove(item);
    }

    public ItemData GetItemAt(int x, int y)
    {
        foreach (ItemData item in inv)
        {
            int w = item.rotated ? item.sizeY : item.sizeX;
            int h = item.rotated ? item.sizeX : item.sizeY;

            if (x >= item.posX && x < item.posX + w &&
                y >= item.posY && y < item.posY + h)
                return item;
        }
        return null;
    }

    public bool TryPlaceWithStacking(ItemData item)
    {
        ItemDefinition def = ItemRegistry.instance.ByID(item.itemID);
        if (def == null)
        {
            return PlaceItem(item);
        }

        ItemData existingStack = null;
        foreach (ItemData other in inv)
        {
            if (other.itemID == item.itemID)
            {
                existingStack = other;
                break;
            }
        }

        if (existingStack != null)
        {
            int totalAmount = existingStack.amount + item.amount;
            if (totalAmount <= def.maxAmount)
            {
                existingStack.amount = totalAmount;
                return true;
            }
            else if (existingStack.amount < def.maxAmount)
            {
                existingStack.amount = def.maxAmount;
                item.amount = totalAmount - def.maxAmount;
                return PlaceItem(item);
            }
        }

        return PlaceItem(item);
    }

    public bool CanStackAt(ItemData movingItem, int x, int y)
    {
        ItemData target = GetItemAt(x, y);
        if (target == null || target == movingItem)
        {
            return false;
        }

        if (target.itemID != movingItem.itemID)
        {
            return false;
        }

        ItemDefinition def = ItemRegistry.instance != null
            ? ItemRegistry.instance.ByID(movingItem.itemID)
            : null;

        if (def == null)
        {
            return false;
        }

        return target.amount < def.maxAmount;
    }

    public bool TryPlaceOrStackAt(ItemData item, int x, int y)
    {
        int originalX = item.posX;
        int originalY = item.posY;
        int originalAmount = item.amount;

        item.posX = x;
        item.posY = y;

        ItemData target = GetItemAt(x, y);
        if (target != null && target != item && target.itemID == item.itemID)
        {
            ItemDefinition def = ItemRegistry.instance != null
                ? ItemRegistry.instance.ByID(item.itemID)
                : null;

            if (def != null && target.amount < def.maxAmount)
            {
                int amountToMerge = Mathf.Min(def.maxAmount - target.amount, item.amount);
                target.amount += amountToMerge;
                item.amount -= amountToMerge;

                if (item.amount <= 0)
                {
                    return true;
                }

                if (PlaceItem(item))
                {
                    return true;
                }

                target.amount -= amountToMerge;
                item.amount = originalAmount;
                item.posX = originalX;
                item.posY = originalY;
                return false;
            }
        }

        if (PlaceItem(item))
        {
            return true;
        }

        item.amount = originalAmount;
        item.posX = originalX;
        item.posY = originalY;
        return false;
    }
}
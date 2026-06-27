using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Inventory
{
    [Serializable]
    public class Grid
    {
        public List<ItemStack> items = new List<ItemStack>();
        public int width;
        public int height;

        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        // Helper method to get item at a specific grid position
        private ItemStack GetItemAtPosition(int x, int y)
        {
            foreach (var item in items)
            {
                if (item.position.x <= x && x < item.position.x + item.Width() &&
                    item.position.y <= y && y < item.position.y + item.Height())
                {
                    return item;
                }
            }
            return null;
        }

        // Check if an item can fit at the specified position
        public bool CanPlaceItem(ItemStack item, int x, int y)
        {
            // Check bounds
            if (x < 0 || y < 0 || x + item.Width() > width || y + item.Height() > height)
                return false;
            
            ItemStack existingItem = GetItemAtPosition(x, y);
            
            if (existingItem != null)
            {
                if (item.myItem == existingItem.myItem)
                {
                    if (existingItem.CanStack(item))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }

            // Check if all required slots are empty
            for (int i = x; i < x + item.Width(); i++)
            {
                for (int j = y; j < y + item.Height(); j++)
                {
                    if (GetItemAtPosition(i, j) != null)
                        return false;
                }
            }

            return true;
        }

        // Place an item at the specified position
        public bool PlaceItem(ItemStack itemStack, int x, int y)
        {
            ItemStack existingItem = GetItemAtPosition(x, y);
            
            if (existingItem != null)
            {
                if (existingItem.AttemptStack(itemStack))
                {
                    // Successfully stacked, no need to place
                    return true;
                }
                else
                {
                    // Cannot place item here
                    return false;
                }
            }
            
            if (!CanPlaceItem(itemStack, x, y))
                return false;

            // Set the position and add to list
            itemStack.position = new Vector2Int(x, y);
            items.Add(itemStack);

            return true;
        }

        // Remove an item from the grid
        public ItemStack RemoveItem(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return null;

            ItemStack itemStack = GetItemAtPosition(x, y);
            if (itemStack == null)
                return null;

            // Remove from list
            items.Remove(itemStack);

            return itemStack;
        }

        // Try to add an item anywhere it fits
        public bool TryAddItem(ItemStack itemStack)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (CanPlaceItem(itemStack, x, y))
                    {
                        PlaceItem(itemStack, x, y);
                        return true;
                    }
                }
            }
            return false;
        }

        // Get the top-left position of an item in the grid
        public bool GetItemPosition(ItemStack itemStack, out int x, out int y)
        {
            if (items.Contains(itemStack))
            {
                x = itemStack.position.x;
                y = itemStack.position.y;
                return true;
            }
            
            x = -1;
            y = -1;
            return false;
        }
    }

    public class ItemStack
    {
        public string ItemID;
        
        public Vector2Int position;

        public Item myItem
        {
            get { return ItemRegistry.instance.GetItem(ItemID); }
        }

        public int Width()
        {
            return myItem.Width;
        }

        public int Height()
        {
            return myItem.Height;
        }

        public int Quantity;

        // Attempt to stack items together
        public bool AttemptStack(ItemStack other)
        {
            Debug.Log("Stack Attempted with " + other.ItemID + " x" + other.Quantity);
            // Can only stack if same item type
            if (ItemID != other.ItemID)
                return false;

            // Check if we can add the quantity without exceeding max stack
            int spaceAvailable = myItem.maxStack - Quantity;
            if (spaceAvailable <= 0)
                return false;

            // Add as much as possible
            int amountToAdd = Math.Min(spaceAvailable, other.Quantity);
            Quantity += amountToAdd;
            other.Quantity -= amountToAdd;

            return true;
        }

        // Check if this stack can accept more items
        public bool CanStack(ItemStack other)
        {
            return ItemID == other.ItemID && Quantity < myItem.maxStack;
        }

        // Get remaining stack space
        public int GetRemainingStackSpace()
        {
            return myItem.maxStack - Quantity;
        }

        // Check if stack is full
        public bool IsFull()
        {
            return Quantity >= myItem.maxStack;
        }

        public ItemStack(string itemID, int quantity = 1)
        {
            ItemID = itemID;
            Quantity = quantity;
        }
    }
}
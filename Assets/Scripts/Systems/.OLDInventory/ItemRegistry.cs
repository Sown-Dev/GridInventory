using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Inventory
{
    public class ItemRegistry : MonoBehaviour
    {
        public static int GridSize = 32;
        
        public static ItemRegistry instance;

        public Transform onTop;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public List<ItemRegistryEntry> items;

        [ContextMenu("Register All Items")]
        public void RegisterAllItems()
        {
            items = new List<ItemRegistryEntry>();
            Item[] allItems = Resources.LoadAll<Item>("Items");
            foreach (Item item in allItems)
            {
                RegisterItem(item);
            }
        }
        
        public void RegisterItem(string itemId, Item item)
        {
            ItemRegistryEntry entry = new ItemRegistryEntry();
            entry.itemId = itemId;
            entry.item = item;
            items.Add(entry);
        }

        public void RegisterItem(Item item)
        {
            ItemRegistryEntry entry = new ItemRegistryEntry();
            entry.itemId = item.name;
            entry.item = item;
            items.Add(entry);
        }

        public Item GetItem(string itemId)
        {
            return  items.Find(x => x.itemId == itemId)?.item;
        }

        public string GetItemID(Item i)
        {
            return i.name;
        }
}
    [Serializable]
    public class ItemRegistryEntry
    {
        public string itemId;
        public Item item;
    }
}
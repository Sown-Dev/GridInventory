using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry instance;

    private Dictionary<int, ItemDefinition> definitions = new Dictionary<int, ItemDefinition>();

    void Awake()
    {
        instance = this;
        LoadDefinitions();
    }

    void LoadDefinitions()
    {
        ItemDefinition[] loaded = Resources.LoadAll<ItemDefinition>("ItemDefinitions");

        foreach (ItemDefinition def in loaded)
        {
            if (definitions.ContainsKey(def.itemID))
            {
                Debug.LogWarning($"Duplicate itemID {def.itemID} found on {def.name}, skipping.");
                continue;
            }
            definitions[def.itemID] = def;
        }

        Debug.Log($"ItemRegistry loaded {definitions.Count} definitions.");
    }

    public ItemDefinition ByID(int id)
    {
        definitions.TryGetValue(id, out ItemDefinition def);
        return def;
    }

    public List<ItemDefinition> GetAllDefinitions()
    {
        return new List<ItemDefinition>(definitions.Values);
    }
}
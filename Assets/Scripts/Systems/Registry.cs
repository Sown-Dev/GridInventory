using System.Collections.Generic;
using UnityEngine;

public class Registry : MonoBehaviour
{
    public static Registry instance;

    private Dictionary<int, ItemDefinition> definitions = new Dictionary<int, ItemDefinition>();
    private Dictionary<int, ComponentDefinition> componentDefinitions = new Dictionary<int, ComponentDefinition>();
    
    void Awake()
    {
        instance = this;
        LoadDefinitions();
    }

    void LoadDefinitions()
    {
        ItemDefinition[] loaded = Resources.LoadAll<ItemDefinition>("ItemDefinitions");
        ComponentDefinition[] loadedComponents = Resources.LoadAll<ComponentDefinition>("itemComponents");

        foreach (ItemDefinition def in loaded)
        {
            if (definitions.ContainsKey(def.itemID))
            {
                Debug.LogWarning($"Duplicate itemID {def.itemID} found on {def.name}, skipping.");
                continue;
            }
            definitions[def.itemID] = def;
        }

        foreach (ComponentDefinition def in loadedComponents)
        {
            if (componentDefinitions.ContainsKey(def.ID))
            {
                Debug.LogWarning($"Duplicate component ID {def.ID} found on {def.name}, skipping.");
                continue;
            }

            componentDefinitions[def.ID] = def;
        }

        Debug.Log($"ItemRegistry loaded {definitions.Count} item definitions and {componentDefinitions.Count} component definitions.");
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

    public ComponentDefinition ByComponentID(int id)
    {
        componentDefinitions.TryGetValue(id, out ComponentDefinition def);
        return def;
    }

    public List<ComponentDefinition> GetAllComponentDefinitions()
    {
        return new List<ComponentDefinition>(componentDefinitions.Values);
    }
}
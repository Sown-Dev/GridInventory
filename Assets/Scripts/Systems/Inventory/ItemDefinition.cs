using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public int itemID;
    [SerializeField, HideInInspector] private string assetGuid;
    public int value=10;
    public int sizeX=1;
    public int sizeY=1;
    public int maxAmount=1;
    public Sprite icon;
    
    public List<ComponentDefinition> componentDefinitions;

    public string description;
    public ItemData GenerateData()
    {
        ItemData item = new ItemData
        {
            itemID = itemID,
            sizeX = sizeX,
            sizeY = sizeY,
            amount = 1,
            value = value,
            rotated = false,
        };

        if (componentDefinitions != null && componentDefinitions.Count > 0)
        {
            List<ItemComponent> components = new List<ItemComponent>(componentDefinitions.Count);

            for (int index = 0; index < componentDefinitions.Count; index++)
            {
                ComponentDefinition componentDefinition = componentDefinitions[index];
                if (componentDefinition == null)
                {
                    continue;
                }

                ItemComponent component = componentDefinition.GenerateComponentS();
                if (component != null)
                {
                    components.Add(component);
                }
            }

            item.Components = components.ToArray();
        }
        else
        {
            item.Components = new ItemComponent[0];
        }

        return item;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        RefreshIdentityFromGuid();
    }

    private void OnValidate()
    {
        RefreshIdentityFromGuid();
    }

    private void RefreshIdentityFromGuid()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
        {
            return;
        }

        int newItemId = GuidToInt(guid);
        bool changed = false;

        if (assetGuid != guid)
        {
            assetGuid = guid;
            changed = true;
        }

        if (itemID != newItemId)
        {
            itemID = newItemId;
            changed = true;
        }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    private static int GuidToInt(string guid)
    {
        unchecked
        {
            const int fnvOffset = unchecked((int)2166136261);
            const int fnvPrime = 16777619;
            int hash = fnvOffset;

            for (int index = 0; index < guid.Length; index++)
            {
                hash ^= guid[index];
                hash *= fnvPrime;
            }

            return hash == 0 ? 1 : hash;
        }
    }
#endif
}
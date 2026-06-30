
using System;
using UnityEngine;

public abstract class ComponentDefinition : ScriptableObject
{
    public int ID;
    [SerializeField, HideInInspector] private string assetGuid;

    public abstract ItemComponent GenerateComponentS();

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

        int newId = GuidToInt(guid);
        bool changed = false;

        if (assetGuid != guid)
        {
            assetGuid = guid;
            changed = true;
        }

        if (ID != newId)
        {
            ID = newId;
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

   


using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Properties;
using Unity.Serialization;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
[GeneratePropertyBag]
public class ItemData
{
    public int posX;
    public int posY;
    public int sizeX;
    public int sizeY;
    public bool rotated;
    public int amount;
    public int itemID;
    public double value;

    [SerializeReference]
    public ItemComponent[] Components;

    [DoNotSerialize, JsonIgnore, DontSerialize]
    public Action OnChanged;
    
    public virtual T GetComponent<T>() where T : ItemComponent
    {
        if (Components == null) return null;

        foreach (var component in Components)
        {
            if (component is T typedComponent)
            {
                return typedComponent;
            }
        }

        return null;
    }
    public virtual bool HasComponent<T>() where T : ItemComponent
    {
        if (Components == null) return false;

        foreach (var component in Components)
        {
            if (component is T)
            {
                return true;
            }
        }

        return false;
    }
    public string GetName()
    {
        return Registry.instance.ByID(itemID).name;
    }
    public string GetDescription()
    {
        return Registry.instance.ByID(itemID).description;
    }
    
    public Sprite GetIcon()
    {
        return Registry.instance.ByID(itemID).icon;
    }
    
    public void InvokeOnChanged()
    {
        try
        {
            OnChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error invoking OnChanged for item {GetName()}: {e}");
        }
    }
}



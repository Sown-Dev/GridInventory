using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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

    public ItemComponent[] Components;
    
    public virtual ItemComponent GetComponent<T>() where T : ItemComponent
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
}



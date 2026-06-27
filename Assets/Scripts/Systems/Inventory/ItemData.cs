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
}

public class WeaponData
{
    public int MagSize = 30;
    public int AmmoCount = 0;
    public Inventory Upgrades;

    public int[] CompatibleAmmo;
    
}


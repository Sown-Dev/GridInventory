

using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Gun", menuName = "Inventory/Weapon Component")]

public class WeaponComponentDefinition : ComponentDefinition
{
    public int MagSize = 30;
    public int AmmoCount = 0;
    public Inventory Upgrades;

    public int[] CompatibleAmmo;

    public override ItemComponent GenerateComponent()
    {
        return new WeaponItemComponent
        {
            definitionID = ID,
            loadedAmmoCount = 0,
        };
    }
}

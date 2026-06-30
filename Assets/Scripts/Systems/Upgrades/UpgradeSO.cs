using System;
using Ligofff.CustomSOIcons;
using UnityEngine;


public class UpgradeSO: ScriptableObject
{
    [CustomAssetIcon]
   public Sprite icon => u.Icon;
    
    [SerializeField] public virtual Upgrade u{
        get;
        set;
    }

    public virtual void OnValidate(){
        u.Name = name;
    }
}
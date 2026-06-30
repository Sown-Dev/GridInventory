using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class Upgrade: IToolTippable{
    
    public string Name;
    
    public Stats st;

    public Sprite Icon;

    public float value = 1;
    public float basePrice = 10;

    public virtual void Init(){
        
    }
    public virtual void Remove(){}

    public Upgrade Clone(){
       return this.MemberwiseClone() as Upgrade;
    }
    
    [TextArea(2,10)]
    public string _description;
    
    public LocalizedString LocalizedName;
    public LocalizedString LocalizedDescription;
    public string name => Application.isPlaying && LocalizedName != null && !LocalizedName.IsEmpty ? LocalizedName.GetLocalizedString() : Name;
    public string description => Application.isPlaying && LocalizedDescription != null && !LocalizedDescription.IsEmpty ? LocalizedDescription.GetLocalizedString() : _description;
    public Sprite icon => Icon;

    public bool FullVersionRequired = false;

    public bool criticalUpgrade = false;
    
   // public bool sold = false;

}
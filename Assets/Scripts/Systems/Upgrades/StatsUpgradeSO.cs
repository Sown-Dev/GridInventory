using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;




[CreateAssetMenu(fileName = "UpgradeSO", menuName = "ScriptableObjects/StatsUpgradeSO", order = 1)]
[Serializable]
public class StatsUpgradeSO : UpgradeSO{
    /*
     this wierd setup allows us to have a wrapper scriptable object for upgrades
    You can edit the serialized field in the inspector
    but the actual upgrade is stored in the upgrade parameter, which is used in the code, allowing us to have polymorphism with scriptable objects (which typically don't support inheritance)
    */
    [SerializeField] private Upgrade su;

    public override Upgrade u{
        get{ return su; }
        set{ su = value; }
    }
}
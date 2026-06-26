using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry: MonoBehaviour{
    public static ItemRegistry instance;
        
    public Dictionary<int,ItemDefinition> Definitions;
        
    public void Awake(){
        instance=this;        
        RegisterItems();
    }
        
    public void RegisterItems(){
                
    }
    public ItemDefinition ByID(int id){
        return Definitions[id];        
    }
   
}
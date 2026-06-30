using System;
using UnityEngine.Serialization;

[Serializable]
public class Statistic{
     public Statstype type =(Statstype)1;
    public double amount;
    public Stats.StatsOperation operation = Stats.StatsOperation.Add;

    public bool hidden;
    
    public Statistic(){

        amount = 0;
        operation = Stats.StatsOperation.Add;
    }
    

    public Statistic(Statstype _type, double amt, Stats.StatsOperation op){
        type = _type;
        amount = amt;
        operation = op;
    }
    
   
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum Statstype{
    
    //Core
    
    MaxHealth = 1,
    Armor = 2,
    InventoryRows = 3,
    
    //Movement
    
    MoveSpeed = 11,
    JumpHeight = 12,
    
    //Combat
    
    Damage = 21,
    AttackSpeed = 22,
}

[Serializable]
public class Stats : ICloneable{
    public List<Statistic> stats;
    
    

    public Stats(){
        stats = new List<Statistic>();
    }

    public Stats(int i){
        stats = new List<Statistic>();  
        foreach (Statstype t in Enum.GetValues(typeof(Statstype))){
            stats.Add(new Statistic(t, i, StatsOperation.Add));
        }
    }

    [Serializable]
    public enum StatsOperation{
        Multiply = 1,
        Add = 2,
    }

    public Stats Combine(Stats toCombine){
        if(toCombine == null){
            return this;
        }
        
        /*foreach (KeyValuePair<Statstype, Statistic> e  in toCombine.stats){
            if (this.stats[e.Key] != null){
                if (e.Value.operation == StatsOperation.Add){
                    this.stats[e.Key].amount += e.Value.amount;
                }
                if(e.Value.operation == StatsOperation.Multiply){
                    this.stats[e.Key].amount *= e.Value.amount ;
                }
            }else{
                this.stats[e.Key] = e.Value;
            }
        }
        return this;*/
        foreach (var combiningStat in toCombine.stats){
            bool found = false;
            foreach (var myStat in this.stats.Where(f => f.type == combiningStat.type)){
                found = true;
                //if both multiply, end result is multiply, if one is add and one mult, end is add, else add
                // ( * * => * ) ; ( * + => + ; + * => * ) ; ( + + => + )
                if (combiningStat.operation == StatsOperation.Multiply && myStat.operation == StatsOperation.Multiply){
                    myStat.amount *= combiningStat.amount;
                    myStat.operation = StatsOperation.Multiply;
                }
                else if (combiningStat.operation == StatsOperation.Multiply && myStat.operation == StatsOperation.Add){
                    myStat.amount *= combiningStat.amount;
                    myStat.operation = StatsOperation.Add;
                }
                else if (combiningStat.operation == StatsOperation.Add && myStat.operation == StatsOperation.Multiply){
                    myStat.amount *= combiningStat.amount;
                    myStat.operation = StatsOperation.Add;
                }
                else if (combiningStat.operation == StatsOperation.Add && myStat.operation == StatsOperation.Add){
                    myStat.amount += combiningStat.amount;
                    myStat.operation = StatsOperation.Add;
                }
                //doesn't break in case you have multiple stats of the same type
            }

            if (!found){
                this.stats.Add(combiningStat);
            }
        }

        return this;
    }

    //rewrite this for null case
    public float this[Statstype stat]{
        get{
            if (this.stats.Find(t => t.type == stat) != null){
                return (float)this.stats.Find(t => t.type == stat).amount;
            }
            else{
                return 0;
            }
        }
        set{
            if (this.stats.Find(t => t.type == stat) != null){
                this.stats.Find(t => t.type == stat).amount = value;
            }
            else{
                this.stats.Add(new Statistic(){ type = stat, amount = value, operation = StatsOperation.Multiply });
            }
        }
    }

    public int GetInt(Statstype stat){
        return Mathf.RoundToInt(this[stat]);
    }

    public bool BoolStat(Statstype stat){
        return this[stat] > 0;
    }

    public object Clone(){
        Stats s = new Stats();
        foreach (var e in this.stats){
            s.stats.Add(new Statistic(){ type = e.type, amount = e.amount, operation = e.operation });
        }

        return s;
    }

    public static Stats operator *(Stats a, float b){
        Stats ret = new Stats();

        foreach (Statistic s in a.stats){
            ret[s.type] = (float)(s.amount * b);
        }

        return a;
    }
    
    public new string ToString(){
        string ret = "";
        foreach (Statistic s in this.stats){
            string op = s.operation == StatsOperation.Add ? "+" : "*";
            ret += s.type.ToString() + ": "+op + s.amount.ToString() + "\n";
        }
        return ret;
    }
}
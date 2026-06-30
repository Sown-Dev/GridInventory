
    public class StatsUnit: Unit
    {
        public override int maxHealth => finalStats.GetInt(Statstype.MaxHealth);

        public Stats baseStats;
        public Stats finalStats;
    
        public override void Start()
        {
            CalculateStats();
            base.Start();
        }
        
        public virtual void CalculateStats()
        {
            finalStats = new Stats(0);
            finalStats.Combine(baseStats);
            
            ApplyStats();
        }

        public virtual void ApplyStats()
        {
            //might not be necessary if we use property.
            maxHealth = finalStats.GetInt(Statstype.MaxHealth);

        }
    }

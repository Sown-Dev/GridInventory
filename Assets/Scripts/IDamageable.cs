namespace DefaultNamespace
{
    public interface IDamageable
    {
        public Faction faction { get; }
        public void TakeDamage(float damage);
    }

    public enum Faction
    {
        Friendly,
        Drone
    }
}
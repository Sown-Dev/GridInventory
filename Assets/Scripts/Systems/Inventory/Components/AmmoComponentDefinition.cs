

//Ammo component:
//Defines bullet properties. Common changes will be damage, velocity, recoil. , 
    public class AmmoComponentDefinition: ComponentDefinition
    {
        public Caliber myCaliber;
        public int velocityAddition;
        public Stats ammoStats;
        
        public override ItemComponent GenerateComponents(ItemData itemData)
        {
            return new AmmoItemComponent
            {
                definitionID = ID,
                myItemData = itemData
            };
        }    }

    public enum Caliber
    {
        _9mm,
        _45ACP,
        _556NATO,
        ArrowBolt,
    }
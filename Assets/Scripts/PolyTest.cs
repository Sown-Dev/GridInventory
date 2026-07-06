using UnityEngine;
using Unity.Properties;               // Required for property attributes
using Unity.Serialization.Json;     // Required for JsonSerialization

public class UnitySerializationPolymorphismTest : MonoBehaviour
{
    // 1. Define the polymorphic classes using Property Bags
    [GeneratePropertyBag]
    public class Animal
    {
        public string speciesName = "Unknown Animal";
    }

    [GeneratePropertyBag]
    public class Dog : Animal
    {
        public string barkSound = "Woof!";
        public bool lovesFetch = true;
    }

    // 2. The container class holding the polymorphic field
    [GeneratePropertyBag]
    public class ZooContainer
    {
        // A base class reference holding a derived object
        public Animal residentAnimal;
    }

    private void Awake()
    {
        Debug.Log("=== STARTING COM.UNITY.SERIALIZATION TEST ===");

        // Setup our polymorphic data structure
        ZooContainer myZoo = new ZooContainer();
        myZoo.residentAnimal = new Dog 
        { 
            speciesName = "Canine", 
            barkSound = "Bork Bork!", 
            lovesFetch = true 
        };

        // ==========================================
        // STEP 1: SERIALIZATION (Object -> JSON)
        // ==========================================
        // Fixed: Swapped Serialize() out for ToJson()
        string jsonOutput = JsonSerialization.ToJson(myZoo);
        Debug.Log("Generated JSON Output:\n" + jsonOutput);

        // ==========================================
        // STEP 2: DESERIALIZATION (JSON -> Object)
        // ==========================================
        // Fixed: Swapped Deserialize() out for FromJson()
        ZooContainer restoredZoo = JsonSerialization.FromJson<ZooContainer>(jsonOutput);

        // Check if the restored object successfully remembered it's a Dog!
        if (restoredZoo.residentAnimal is Dog restoredDog)
        {
            Debug.Log($"<color=green>SUCCESS!</color> Deserialized type is a Dog.");
            Debug.Log($"Restored Dog Data: Bark='{restoredDog.barkSound}', Fetch={restoredDog.lovesFetch}");
        }
        else
        {
            Debug.Log("<color=red>FAILED!</color> It fell back to a base Animal.");
        }

        Debug.Log("=== TEST COMPLETE ===");
    }
}
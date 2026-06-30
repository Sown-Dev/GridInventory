using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IBuildingComponent
{
    void OnBuildingInit();
    void OnBuildingSpawn();
    void OnBuildingSave();
    void OnBuildingLoad();
    void Load();
    
    
}

// Optional: Base class for common component functionality
public abstract class BaseBuildingComponent : MonoBehaviour, IBuildingComponent
{
    
  

    public virtual void OnBuildingInit()
    {
    }
    public virtual void OnBuildingSpawn() { }

    public virtual void OnBuildingSave()
    {

    }

    public virtual void OnBuildingLoad()
    {
        Load();
    }

    public virtual void Load()
    {

    }

}
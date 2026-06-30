using UnityEngine;
using UnityEngine.Events;
/*
[RequireComponent(typeof(HighlightNearComponent))]
public class InteractableComponent : BaseBuildingComponent
{
    
    
    [Header("Events")]
    public UnityEvent OnInteracted;
    
    private HighlightNearComponent highlightComponent;

    public bool singleInteract = false;

    private void Start()
    {
        // Get required highlight component
        highlightComponent = GetComponent<HighlightNearComponent>();
        
        if (highlightComponent == null)
        {
            Debug.LogError($"InteractableComponent on {gameObject.name} requires HighlightNearComponent!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // Only check for input if highlighted
        if (highlightComponent.IsOpen && GameInput.InteractPressed )//&& Utils.interactTimer <= 0f)
        {
            Interact();
            //Utils.interactTimer = 0.1f;
        }
    }

    public virtual void Interact()
    {
        if (singleInteract)
        {
            if (Utils.interactedThisFrame)
            {
                return;
            }

            Utils.interactedThisFrame = true;
        }

        Debug.Log($"Interact w{gameObject.name}, interactedThisFrame={Utils.interactedThisFrame}");
        OnInteracted?.Invoke();
    }

    // Simple accessor for highlight state
    public bool IsHighlighted => highlightComponent != null && highlightComponent.IsOpen;
}*/
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public readonly UnityAction OnInteract;
    
    [SerializeField] private SpriteRenderer sr;
    
    private bool isOutlineActive = false;
    
    private Material mat;

    public Interactable(UnityAction onInteract)
    {
        OnInteract = onInteract;
    }

    public virtual void Start()
    {
        if (sr == null)
        {
            sr = gameObject.GetComponent<SpriteRenderer>();
        }

        if (sr != null)
        {
            // Clone the material so modifying the outline doesn't affect all other sprites
            mat = new Material(sr.material);
            sr.material = mat;
            
            SetOutline(false);
        }
    }
    
    public virtual void Update()
    {
        if (Player.instance == null) return;

        bool inRange = Vector2.Distance(Player.instance.transform.position, transform.position) <= 2;
        
        if (inRange)
        {
            if (!isOutlineActive)
            {
                SetOutline(true);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
        else
        {
            if (isOutlineActive)
            {
                SetOutline(false);
            }
        }
    }
    
    public virtual void Interact()
    {
        OnInteract?.Invoke();   
    }

    private void SetOutline(bool active)
    {
        if (mat == null) return;

        isOutlineActive = active;
        mat.SetFloat("_OutlineThickness", active ? 1f : 0f);
        
        if (active)
        {
            mat.SetColor("_OutlineColor", Color.white);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class HighlightNearComponent : BaseBuildingComponent
{
    [Header("Highlight Configuration")] public float distance = 2.5f;
    public Color highlightColor = Color.white;

    [Header("Events")] public UnityEvent OnBuildingOpened;
    public UnityEvent OnBuildingClosed;

    private Transform player;
    private bool isOpen = false;
    private Material material;
    private SpriteRenderer spriteRenderer;

    [FormerlySerializedAs("useWorldUI")] [SerializeField] private bool useMiningUI = false;
    
    public bool focusCamera = true;
/*
    private void Start()
    {
        player = Player.instance.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            material = spriteRenderer.material;
        }
    }

    private void Update()
    {
        if (player == null || material == null) return;

        bool shouldBeOpen = Vector2.Distance(player.position, transform.position) < distance &&
                            (player.position.y >= transform.position.y);

        if (shouldBeOpen && !isOpen)
        {
            OpenBuilding();
        }
        else if (!shouldBeOpen && isOpen)
        {
            CloseBuilding();
        }
    }

    private void OpenBuilding()
    {
        UIDepthManager.Instance.OpenWorldUI();
        
        if(useMiningUI)
            UIDepthManager.Instance.OpenMiningUI();


        material.SetColor("_ReplaceColor", highlightColor);
        isOpen = true;

        // Default behavior - set camera target
        if (focusCamera)
            GameManager.instance.SetCameraTarget(transform, active: true, priority: 10, offset: Player.surfaceOffset);

        OnBuildingOpened?.Invoke();
    }

    private void CloseBuilding()
    {
        UIDepthManager.Instance.CloseWorldUI();
        if(useMiningUI)
            UIDepthManager.Instance.CloseMiningUI();

        material.SetColor("_ReplaceColor", Color.clear);
        isOpen = false;

        // Default behavior - remove camera target
        if (focusCamera)
            GameManager.instance.SetCameraTarget(transform, active: false);

        OnBuildingClosed?.Invoke();
    }

    public bool IsOpen => isOpen;

    // Public methods for manual control
    public void ForceOpen()
    {
        if (!isOpen) OpenBuilding();
    }

    public void ForceClose()
    {
        if (isOpen) CloseBuilding();
    }

    void OnDisable()
    {
        if (isOpen)
        {
            try
            {
                CloseBuilding();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if(useMiningUI)
                    UIDepthManager.Instance.CloseMiningUI();

                UIDepthManager.Instance.CloseWorldUI();
            }
        }
    }*/

}
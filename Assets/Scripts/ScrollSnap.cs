using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollSnap : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] private float incrementSize = 32f;
    [SerializeField] private float snapSpeed = 15f;
    [SerializeField] private float velocityThreshold = 100f; // Velocity below this triggers a snap
    
    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform content;
    
    private bool isSnapping;
    private Vector2 targetPosition;
    private bool isDragging;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRectTransform = GetComponent<RectTransform>();
        content = scrollRect.content;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        // Don't kill velocity here anymore, let inertia run so scroll wheel and flicks feel smooth
    }

    void Update()
    {
        if (isDragging) return;

        // If the content is moving slowly (from wheel scroll or drag inertia) and not already snapping, calculate the snap target
        if (!isSnapping && scrollRect.velocity.magnitude > 0 && scrollRect.velocity.magnitude < velocityThreshold)
        {
            CalculateSnapTarget();
        }

        // Smoothly interpolate to the target position
        if (isSnapping)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            if (Vector2.Distance(content.anchoredPosition, targetPosition) < 0.1f)
            {
                content.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    private void CalculateSnapTarget()
    {
        // Kill the remaining velocity so it stops fighting the lerp
        scrollRect.velocity = Vector2.zero; 
        targetPosition = content.anchoredPosition;
        
        Vector2 minBounds = Vector2.zero;
        Vector2 maxBounds = Vector2.zero;

        if (scrollRect.horizontal)
        {
            float contentWidth = content.rect.width;
            float viewWidth = scrollRectTransform.rect.width;
            float maxScrollX = Mathf.Max(0, contentWidth - viewWidth);
            
            minBounds.x = -maxScrollX;
            maxBounds.x = 0;

            if (targetPosition.x < minBounds.x) targetPosition.x = minBounds.x;
            else if (targetPosition.x > maxBounds.x) targetPosition.x = maxBounds.x;
            else targetPosition.x = Mathf.Round(targetPosition.x / incrementSize) * incrementSize;
        }
        
        if (scrollRect.vertical)
        {
            float contentHeight = content.rect.height;
            float viewHeight = scrollRectTransform.rect.height;
            float maxScrollY = Mathf.Max(0, contentHeight - viewHeight);
            
            minBounds.y = 0;
            maxBounds.y = maxScrollY;

            if (targetPosition.y < minBounds.y) targetPosition.y = minBounds.y;
            else if (targetPosition.y > maxBounds.y) targetPosition.y = maxBounds.y;
            else targetPosition.y = Mathf.Round(targetPosition.y / incrementSize) * incrementSize;
        }

        isSnapping = true;
    }
}
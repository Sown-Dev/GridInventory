using UnityEngine;

public static class ItemContainerUIUtility
{
    public static IItemContainerUI ResolveContainerInParents(Transform start)
    {
        if (start == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = start.GetComponentsInParent<MonoBehaviour>(true);
        for (int index = 0; index < behaviours.Length; index++)
        {
            if (behaviours[index] is IItemContainerUI container)
            {
                return container;
            }
        }

        return null;
    }

    public static IItemContainerUI ResolveContainerAtScreenPoint(Vector2 screenPosition)
    {
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        IItemContainerUI bestContainer = null;
        float bestArea = float.MaxValue;

        for (int index = 0; index < behaviours.Length; index++)
        {
            if (behaviours[index] is not IItemContainerUI container)
            {
                continue;
            }

            if (!container.ContainsScreenPoint(screenPosition))
            {
                continue;
            }

            RectTransform rectTransform = behaviours[index] is GridInventoryUI gridUI
                ? gridUI.ContainerRect
                : behaviours[index] is InventorySlotUI slotUI
                    ? slotUI.ContainerRect
                    : behaviours[index].transform as RectTransform;
            float area = rectTransform != null
                ? Mathf.Abs(rectTransform.rect.width * rectTransform.rect.height)
                : float.MaxValue;

            if (area < bestArea)
            {
                bestArea = area;
                bestContainer = container;
            }
        }

        return bestContainer;
    }
}
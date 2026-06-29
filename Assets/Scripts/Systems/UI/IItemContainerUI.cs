using UnityEngine;

public interface IItemContainerUI
{
    bool ContainsScreenPoint(Vector2 screenPosition);
    bool TryRemoveItem(ItemData item);
    bool CanAcceptItem(ItemData item, Vector2 screenPosition);
    void UpdateDropPreview(ItemData item, Vector2 screenPosition, bool valid);
    void ClearDropPreview();
    bool TryPlaceItem(ItemData item, Vector2 screenPosition);
    bool TryRestoreItem(ItemData item);
    void RefreshView();
}
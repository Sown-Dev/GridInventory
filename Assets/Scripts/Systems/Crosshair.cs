using UnityEngine;

public class Crosshair : MonoBehaviour
{
    void Start()
    {
        // Hides the default OS mouse cursor so they don't overlap
        Cursor.visible = false;
    }

    void Update()
    {
        // Convert screen mouse position to world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Lock the Z axis to 0 for 2D space
        mousePosition.z = 0f;
        
        // Update the crosshair object's position
        transform.position = mousePosition;
    }
}
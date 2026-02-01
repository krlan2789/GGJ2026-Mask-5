using UnityEngine;

[DisallowMultipleComponent]
public class CursorFollow : MonoBehaviour
{
    void Update()
    {
        // Convert directly to world position in 2D
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Ensure z is 0 for 2D
        mousePos.z = 0;

        // Direction from sprite to mouse
        Vector2 direction = mousePos - transform.position;

        // Calculate angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

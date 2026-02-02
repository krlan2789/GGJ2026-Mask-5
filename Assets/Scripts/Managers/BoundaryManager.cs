using UnityEngine;

[DisallowMultipleComponent]
public class BoundaryManager : MonoBehaviour
{
    [SerializeField] private float _minX = -10f;
    [SerializeField] private float _maxX = 10f;
    [SerializeField] private float _minY = -5f;
    [SerializeField] private float _maxY = 5f;
    [SerializeField] private bool _showBounds = true;
    [SerializeField] private Color _boundsColor = new Color(0, 1, 0, 0.5f);

    private void Start()
    {
        // Validate boundaries
        if (_maxX <= _minX)
            Debug.LogError("BoundaryManager: _maxX must be greater than _minX!");
        if (_maxY <= _minY)
            Debug.LogError("BoundaryManager: _maxY must be greater than _minY!");
    }

    public Vector2 ConstrainPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, _minX, _maxX),
            Mathf.Clamp(position.y, _minY, _maxY)
        );
    }

    public Bounds GetBounds()
    {
        Vector2 center = new Vector2((_minX + _maxX) / 2, (_minY + _maxY) / 2);
        Vector2 size = new Vector2(_maxX - _minX, _maxY - _minY);
        return new Bounds(center, size);
    }

    private void OnDrawGizmos()
    {
        if (!_showBounds)
            return;

        // Draw boundary rectangle in Scene view
        Vector3 topLeft = new Vector3(_minX, _maxY, 0);
        Vector3 topRight = new Vector3(_maxX, _maxY, 0);
        Vector3 bottomRight = new Vector3(_maxX, _minY, 0);
        Vector3 bottomLeft = new Vector3(_minX, _minY, 0);

        Gizmos.color = _boundsColor;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
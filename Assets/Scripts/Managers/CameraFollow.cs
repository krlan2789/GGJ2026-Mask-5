using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private Vector3 _offset = new(0, 0, -10f);
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = true;

    private Camera _camera;
    private PlayerMovement _playerMovement;
    private BoundaryManager _boundaryManager;
    private Transform _playerTransform;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        _boundaryManager = FindFirstObjectByType<BoundaryManager>();
        if (_playerMovement != null) _playerTransform = _playerMovement.gameObject.transform;

        if (_camera == null)
            Debug.LogError("Camera component not found on CameraFollow GameObject!");
        if (_playerTransform == null)
            Debug.LogError("PlayerMovement not found in scene!");
        if (_boundaryManager == null)
            Debug.LogError("BoundaryManager not found in scene!");
    }

    private void LateUpdate()
    {
        if (_playerTransform == null || _camera == null)
            return;

        // Calculate desired position with offset
        Vector3 desiredPosition = _playerTransform.position + _offset;

        // Apply axis restrictions
        if (!_followX)
            desiredPosition.x = transform.position.x;
        if (!_followY)
            desiredPosition.y = transform.position.y;

        // Smoothly move camera towards desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);

        // Constrain camera to boundaries (if BoundaryManager is available)
        if (_boundaryManager != null)
        {
            smoothedPosition = ConstrainCameraPosition(smoothedPosition);
        }

        transform.position = smoothedPosition;
    }

    private Vector3 ConstrainCameraPosition(Vector3 cameraPosition)
    {
        float cameraHalfHeight = _camera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * _camera.aspect;

        Bounds bounds = _boundaryManager.GetBounds();
        Vector2 boundsCenter = bounds.center;
        Vector2 boundsSize = bounds.size;

        float minX = boundsCenter.x - boundsSize.x / 2 + cameraHalfWidth;
        float maxX = boundsCenter.x + boundsSize.x / 2 - cameraHalfWidth;
        float minY = boundsCenter.y - boundsSize.y / 2 + cameraHalfHeight;
        float maxY = boundsCenter.y + boundsSize.y / 2 - cameraHalfHeight;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minX, maxX);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minY, maxY);

        return cameraPosition;
    }
}
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GroundSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional. If not set the spawner will try to find the player by tag.")]
    [SerializeField] private Transform _player;

    [Tooltip("Ground prefab to spawn. Should have a Renderer/Collider2D to auto-detect width.")]
    [SerializeField] private GameObject _groundPrefab;

    [Header("Spawn Settings")]
    [Tooltip("How many segments to spawn initially.")]
    [SerializeField] private int _initialSegments = 6;

    [Tooltip("Optional manual width. If <= 0 the width will be taken from the prefab bounds.")]
    [SerializeField] private float _segmentWidth = 0f;

    [Tooltip("Extra distance ahead of the camera's right edge where spawner will ensure ground exists.")]
    [SerializeField] private float _spawnAheadDistance = 8f;

    [Tooltip("Distance behind the camera's left edge at which segments are recycled.")]
    [SerializeField] private float _recycleBehindDistance = 12f;

    [SerializeField] private Transform _poolParent;
    [SerializeField] private int _initialPoolSize = 10;

    private Camera _cam;
    private readonly Queue<GameObject> _pool = new();
    private readonly List<GameObject> _active = new();

    private float _halfWidth;
    private float _groundZDistance;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null) Debug.LogError("GroundSpawner: no Camera.main found.");

        if (_player == null)
        {
            var found = GameObject.FindWithTag(ConstantHelper.Tags.PLAYER);
            if (found != null) _player = found.transform;
        }

        if (_groundPrefab == null)
        {
            Debug.LogError("GroundSpawner: no ground prefab assigned.");
            enabled = false;
            return;
        }

        if (_poolParent == null) _poolParent = transform;

        // Determine segment width from prefab if not manually specified
        if (_segmentWidth <= 0f)
        {
            var prefabRenderer = _groundPrefab.GetComponentInChildren<Renderer>();
            if (prefabRenderer != null)
            {
                _segmentWidth = prefabRenderer.bounds.size.x;
            } else
            {
                var col2D = _groundPrefab.GetComponentInChildren<Collider2D>();
                if (col2D != null)
                    _segmentWidth = col2D.bounds.size.x;
            }

            if (_segmentWidth <= 0f)
            {
                Debug.LogWarning("GroundSpawner: could not determine prefab width, defaulting to 10.");
                _segmentWidth = 10f;
            }
        }

        _halfWidth = _segmentWidth * 0.5f;

        // Prepopulate pool
        for (int i = 0; i < Mathf.Max(_initialPoolSize, _initialSegments + 2); i++)
        {
            var go = CreateInstanceAndDisable();
            _pool.Enqueue(go);
        }

        // Initial spawn centered around player or camera
        float startX;
        if (_player != null)
            startX = _player.position.x - (_initialSegments / 2f) * _segmentWidth;
        else
            startX = transform.position.x - (_initialSegments / 2f) * _segmentWidth;

        for (int i = 0; i < _initialSegments; i++)
        {
            SpawnSegmentAt(startX + i * _segmentWidth);
        }
    }

    private GameObject CreateInstanceAndDisable()
    {
        var go = Instantiate(_groundPrefab, _poolParent);
        go.SetActive(false);
        return go;
    }

    private void SpawnSegmentAt(float worldX)
    {
        GameObject seg;
        if (_pool.Count > 0)
            seg = _pool.Dequeue();
        else
            seg = CreateInstanceAndDisable();

        seg.transform.SetParent(_poolParent, true);
        Vector3 pos = seg.transform.position;
        pos.x = worldX;
        // keep prefab Y/Z by default; align Z to 0 so physics 2D works if needed
        pos.z = seg.transform.position.z;
        seg.transform.position = pos;
        seg.SetActive(true);
        _active.Add(seg);
    }

    private void Update()
    {
        if (_cam == null || _groundPrefab == null || _active.Count == 0) return;

        // compute world-space camera edges on the ground's Z plane
        _groundZDistance = Mathf.Abs(transform.position.z - _cam.transform.position.z);
        Vector3 camLeftWorld = _cam.ViewportToWorldPoint(new Vector3(-1f, 0.5f, _groundZDistance));
        Vector3 camRightWorld = _cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, _groundZDistance));

        float rightEdge = camRightWorld.x;
        float leftEdge = camLeftWorld.x;

        // Ensure ground exists ahead of the camera
        float rightmostX = float.MinValue;
        foreach (var s in _active)
        {
            if (s == null) continue;
            float sx = s.transform.position.x;
            rightmostX = Mathf.Max(rightmostX, sx);
        }

        // If no active segments (shouldn't happen) spawn near player/cam
        if (rightmostX == float.MinValue)
            rightmostX = _player != null ? _player.position.x : transform.position.x;

        // Spawn while rightmost is less than rightEdge + buffer
        while (rightmostX + _halfWidth < rightEdge + _spawnAheadDistance)
        {
            float spawnX = rightmostX + _segmentWidth;
            SpawnSegmentAt(spawnX);
            rightmostX = spawnX;
        }

        // Recycle segments that are far left of the camera
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var s = _active[i];
            if (s == null)
            {
                _active.RemoveAt(i);
                continue;
            }

            float segRight = s.transform.position.x + _halfWidth;
            if (segRight < leftEdge - _recycleBehindDistance)
            {
                // recycle
                s.SetActive(false);
                _active.RemoveAt(i);
                _pool.Enqueue(s);
            }
        }
    }

    // Optional debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _cam == null) return;

        Gizmos.color = Color.green;
        float left = _cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        float right = _cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        Gizmos.DrawLine(new Vector3(left, transform.position.y - 5f, transform.position.z), new Vector3(left, transform.position.y + 5f, transform.position.z));
        Gizmos.DrawLine(new Vector3(right, transform.position.y - 5f, transform.position.z), new Vector3(right, transform.position.y + 5f, transform.position.z));
    }
}
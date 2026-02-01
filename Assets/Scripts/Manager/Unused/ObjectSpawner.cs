using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional. If not set the spawner will try to find the player by tag.")]
    [SerializeField] private Transform _player;

    [Tooltip("Object prefabs to spawn.")]
    [SerializeField] private GameObject[] _objectPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Extra distance ahead of the camera's right edge where spawner will attempt to spawn objects.")]
    [SerializeField] private float _spawnAheadDistance = 8f;

    [Tooltip("Distance behind the camera's left edge at which objects are recycled.")]
    [SerializeField] private float _recycleBehindDistance = 12f;

    [Tooltip("Minimum horizontal spacing between spawned objects.")]
    [SerializeField] private float _minSpacing = 2f;

    [Tooltip("Maximum horizontal spacing between spawned objects.")]
    [SerializeField] private float _maxSpacing = 6f;

    [Tooltip("Chance [0..1] to spawn an objects when a spawn slot is reached.")]
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.85f;

    [Tooltip("Vertical offset from the top of the ground collider when placing an object.")]
    [SerializeField] private float _objectYOffset = 0.0f;

    [Header("Pooling")]
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private Transform _poolParent;

    private Camera _cam;
    private readonly Queue<GameObject> _pool = new();
    private readonly List<GameObject> _active = new();

    private float _groundZDistance;
    private float _nextSpawnX;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null) Debug.LogError("ObjectSpawner: no Camera.main found.");

        if (_player == null)
        {
            var found = GameObject.FindWithTag(ConstantHelper.Tags.PLAYER);
            if (found != null) _player = found.transform;
        }

        if (_objectPrefabs == null || _objectPrefabs.Length == 0)
        {
            Debug.LogError("ObjectSpawner: no object prefabs assigned.");
            enabled = false;
            return;
        }

        if (_poolParent == null) _poolParent = transform;

        // Prepopulate pool
        for (int i = 0; i < Mathf.Max(1, _initialPoolSize); i++)
        {
            var go = CreateInstanceAndDisable();
            _pool.Enqueue(go);
        }

        // Initialize next spawn X near player or camera
        float startX = _player != null ? _player.position.x : transform.position.x;
        _nextSpawnX = startX;
    }

    private GameObject CreateInstanceAndDisable()
    {
        // Instantiate first prefab as template; will be reparented and reused
        var prefab = _objectPrefabs[0];
        var go = Instantiate(prefab, _poolParent);
        go.SetActive(false);
        return go;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        // Try to reuse pool object that matches prefab type (optional).
        // For simplicity reuse any pooled object and replace its prefab content by instantiating when needed.
        if (_pool.Count > 0)
        {
            var go = _pool.Dequeue();
            // If pooled object is of different prefab type we can destroy and create correct type.
            // Here we keep it simple: if the pooled object's name doesn't contain prefab name, re-instantiate.
            if (!go.name.StartsWith(prefab.name))
            {
                Destroy(go);
                go = Instantiate(prefab, _poolParent);
            }
            return go;
        }

        return Instantiate(prefab, _poolParent);
    }

    private void Update()
    {
        if (_cam == null || _objectPrefabs == null || _objectPrefabs.Length == 0) return;

        // compute world-space camera edges at the spawner's Z plane
        _groundZDistance = Mathf.Abs(transform.position.z - _cam.transform.position.z);
        Vector3 camLeftWorld = _cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, _groundZDistance));
        Vector3 camRightWorld = _cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, _groundZDistance));

        float rightEdge = camRightWorld.x;
        float leftEdge = camLeftWorld.x;

        // Spawn obstacles while next spawn X is within spawn window
        while (_nextSpawnX < rightEdge + _spawnAheadDistance)
        {
            // determine next slot
            float spacing = Random.Range(_minSpacing, _maxSpacing);
            _nextSpawnX += spacing;

            if (Random.value > _spawnProbability)
                continue; // skip this slot

            // Find ground at spawn X to place obstacle on top of it
            float spawnY = FindGroundTopAtX(_nextSpawnX, out bool foundGround);
            if (!foundGround)
            {
                // fallback to player's Y or spawner Y if no ground found
                spawnY = _player != null ? _player.position.y : transform.position.y;
            }
            spawnY += _objectYOffset;

            // Choose random prefab
            var prefab = _objectPrefabs[Random.Range(0, _objectPrefabs.Length)];
            var obj = GetFromPool(prefab);
            obj.transform.SetParent(_poolParent, true);
            obj.transform.position = new Vector3(_nextSpawnX, spawnY, obj.transform.position.z);
            obj.SetActive(true);
            _active.Add(obj);
        }

        // Recycle obstacles that are far left of the camera
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var o = _active[i];
            if (o == null)
            {
                _active.RemoveAt(i);
                continue;
            }

            // attempt to get obstacle width for more precise recycling; fall back to object's position
            float halfWidth = 0f;
            var r = o.GetComponentInChildren<Renderer>();
            if (r != null) halfWidth = r.bounds.size.x * 0.5f;

            float objRight = o.transform.position.x + halfWidth;
            if (objRight < leftEdge - _recycleBehindDistance)
            {
                o.SetActive(false);
                _active.RemoveAt(i);
                _pool.Enqueue(o);
            }
        }
    }

    /// <summary>
    /// Tries to find a ground collider at the given world X and returns the top Y coordinate.
    /// Uses GameObjects tagged with ConstantHelper.Tags.GROUND and their Collider2D bounds.
    /// </summary>
    private float FindGroundTopAtX(float worldX, out bool found)
    {
        found = false;
        float topY = 0f;

        var grounds = GameObject.FindGameObjectsWithTag(ConstantHelper.Tags.GROUND);
        if (grounds == null || grounds.Length == 0) return topY;

        foreach (var g in grounds)
        {
            if (g == null) continue;
            var col = g.GetComponentInChildren<Collider2D>();
            if (col == null) continue;

            var b = col.bounds;
            if (b.min.x <= worldX && worldX <= b.max.x)
            {
                topY = b.max.y;
                found = true;
                return topY;
            }
        }

        return topY;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _cam == null) return;

        Gizmos.color = Color.red;
        float left = _cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        float right = _cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        Gizmos.DrawLine(new Vector3(left, transform.position.y - 5f, transform.position.z), new Vector3(left, transform.position.y + 5f, transform.position.z));
        Gizmos.DrawLine(new Vector3(right, transform.position.y - 5f, transform.position.z), new Vector3(right, transform.position.y + 5f, transform.position.z));
    }
}
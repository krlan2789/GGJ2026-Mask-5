using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional. If not set the spawner will try to find the player by tag.")]
    [SerializeField] private Transform _player;

    [Tooltip("Enemy prefabs to spawn.")]
    [SerializeField] private GameObject[] _enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Extra distance ahead of the camera's right edge where spawner will spawn enemies.")]
    [SerializeField] private float _spawnAheadDistance = 12f;

    [Tooltip("Distance behind the camera's left edge at which enemies are recycled.")]
    [SerializeField] private float _recycleBehindDistance = 16f;

    [Tooltip("Minimum horizontal spacing between spawned enemies.")]
    [SerializeField] private float _minSpacing = 3f;

    [Tooltip("Maximum horizontal spacing between spawned enemies.")]
    [SerializeField] private float _maxSpacing = 8f;

    [Tooltip("Chance [0..1] to spawn an enemy when a spawn slot is reached.")]
    [Range(0f, 1f)]
    [SerializeField] private float _spawnProbability = 0.7f;

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
        if (_cam == null) Debug.LogError("EnemySpawner: no Camera.main found.");

        if (_player == null)
        {
            var found = GameObject.FindWithTag(ConstantHelper.Tags.PLAYER);
            if (found != null) _player = found.transform;
        }

        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: no enemy prefabs assigned.");
            enabled = false;
            return;
        }

        if (_poolParent == null) _poolParent = transform;

        // Prepopulate pool with instances of the first prefab (we reuse / reparent as needed)
        for (int i = 0; i < Mathf.Max(1, _initialPoolSize); i++)
        {
            var go = Instantiate(_enemyPrefabs[0], _poolParent);
            go.SetActive(false);
            _pool.Enqueue(go);
        }

        // Start spawn X near player or spawner position
        float startX = _player != null ? _player.position.x : transform.position.x;
        _nextSpawnX = startX;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (_pool.Count > 0)
        {
            var go = _pool.Dequeue();
            // If dequeued object isn't the right type (name mismatch), destroy and instantiate correct prefab.
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
        if (_cam == null || _enemyPrefabs == null || _enemyPrefabs.Length == 0) return;

        // compute world-space camera edges at the spawner's Z plane
        _groundZDistance = Mathf.Abs(transform.position.z - _cam.transform.position.z);
        Vector3 camLeftWorld = _cam.ViewportToWorldPoint(new Vector3(-2.5f, 0.5f, _groundZDistance));
        Vector3 camRightWorld = _cam.ViewportToWorldPoint(new Vector3(-1.5f, 0.5f, _groundZDistance));

        float rightEdge = camRightWorld.x;
        float leftEdge = camLeftWorld.x;

        // Spawn enemies while next spawn X is within spawn window
        while (_nextSpawnX < rightEdge + _spawnAheadDistance)
        {
            float spacing = Random.Range(_minSpacing, _maxSpacing);
            _nextSpawnX += spacing;

            if (Random.value > _spawnProbability)
                continue;

            // Find ground top at spawn X
            float spawnY = FindGroundTopAtX(_nextSpawnX, out bool foundGround);
            if (!foundGround)
            {
                spawnY = _player != null ? _player.position.y : transform.position.y;
            }

            var prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            var obj = GetFromPool(prefab);
            obj.transform.SetParent(_poolParent, true);
            obj.transform.position = new Vector3(_nextSpawnX, spawnY, obj.transform.position.z);
            obj.SetActive(true);

            // If the enemy has an Enemy component, initialize its player reference (optional)
            var enemyComp = obj.GetComponent<EnemyMovement>();
            if (enemyComp != null && _player != null)
            {
                enemyComp.Initialize(_player);
            }

            _active.Add(obj);
        }

        // Recycle enemies far left of the camera
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var e = _active[i];
            if (e == null)
            {
                _active.RemoveAt(i);
                continue;
            }

            float halfWidth = 0f;
            var r = e.GetComponentInChildren<Renderer>();
            if (r != null) halfWidth = r.bounds.size.x * 0.5f;

            float objRight = e.transform.position.x + halfWidth;
            if (objRight < leftEdge - _recycleBehindDistance)
            {
                e.SetActive(false);
                _active.RemoveAt(i);
                _pool.Enqueue(e);
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

        Gizmos.color = Color.magenta;
        float left = _cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        float right = _cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(transform.position.z - _cam.transform.position.z))).x;
        Gizmos.DrawLine(new Vector3(left, transform.position.y - 5f, transform.position.z), new Vector3(left, transform.position.y + 5f, transform.position.z));
        Gizmos.DrawLine(new Vector3(right, transform.position.y - 5f, transform.position.z), new Vector3(right, transform.position.y + 5f, transform.position.z));
    }
}
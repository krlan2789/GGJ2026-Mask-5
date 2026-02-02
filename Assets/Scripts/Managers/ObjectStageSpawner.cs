using UnityEngine;

public class ObjectStageSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField] private Transform[] _targets;

    private void Start()
    {
        foreach (var target in _targets)
        {
            CreateObject(target);
        }
    }

    private void CreateObject(Transform target)
    {
        if (_prefabs.Length == 0) return;
        int index = Random.Range(0, _prefabs.Length);
        GameObject prefab = _prefabs[index];
        Instantiate(prefab, target.position, Quaternion.identity, target);
    }
}

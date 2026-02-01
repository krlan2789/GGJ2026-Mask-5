using UnityEngine;

public class EnemyStageSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField] private Transform[] _targets;

    private void Start()
    {
        CreateEnemy(_targets[Random.Range(0, _targets.Length)]);
    }

    private void CreateEnemy(Transform target)
    {
        if (_prefabs.Length == 0) return;
        int index = Random.Range(0, _prefabs.Length);
        GameObject prefab = _prefabs[index];
        Instantiate(prefab, target.position, Quaternion.identity, target);
    }
}

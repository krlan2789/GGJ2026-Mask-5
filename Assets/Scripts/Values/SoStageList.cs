using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage List", menuName = "Scriptable Objects/Stage List")]
public class SoStageList : ScriptableObject
{
    [SerializeField] private List<GameObject> _stages = new();

    public List<GameObject> StageList => _stages;

    public GameObject GetStage(int level)
    {
        if (level < 1 || level > _stages.Count) return null;
        return _stages[level - 1];
    }
}

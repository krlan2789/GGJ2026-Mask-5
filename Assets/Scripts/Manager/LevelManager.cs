using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private SoStageList _stageList;

    [SerializeField] private byte _maxLevel = 5;
    public byte MaxLevel => _maxLevel;


    private byte _level = 1;
    public byte Level
    {
        get
        {
            _level = (byte)PlayerPrefs.GetInt("level", _level);
            return _level;
        }
        private set
        {
            _level = value;
            PlayerPrefs.SetInt("level", _level);
        }
    }

    public event Action<byte> OnLevelChanged;

    private void Start()
    {
        Level = 0;
        _maxLevel = (byte)_stageList.StageList.Count;
    }

    public void LoadLevelStage()
    {
        var stage = _stageList.GetStage(Level);
        if (stage != null)
        {
            foreach (var oldStage in GameObject.FindGameObjectsWithTag(ConstantHelper.Tags.STAGE))
            {
                Destroy(oldStage);
            }
            var arena = Instantiate(stage);
            var playerSpawn = arena.transform.Find("PlayerSpawnPoint");
            _player.transform.position = playerSpawn.position;
            Debug.Log("LevelManager.LoadLevelStage");
        }
        else
        {
            Debug.LogWarning($"Stage for level {Level} not found in StageList.");
        }
    }

    public void LevelUp()
    {
        Level++;
        if (Level > _maxLevel) Level = MaxLevel;
        OnLevelChanged?.Invoke(Level);
        FindFirstObjectByType<ScoreManager>().UpdateLevel(Level);
    }

    public void LevelDown()
    {
        Level--;
        if (Level < 1) Level = 1;
        OnLevelChanged?.Invoke(Level);
        FindFirstObjectByType<ScoreManager>().UpdateLevel(Level);
    }

    public void ResetLevel()
    {
        Level = 1;
        OnLevelChanged?.Invoke(Level);
        FindFirstObjectByType<ScoreManager>().UpdateLevel(Level);
    }
}

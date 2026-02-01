using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Text _scoreTxt;
    [SerializeField] private Text _levelTxt;

    [SerializeField] private Transform _panelGameOverObj;
    [SerializeField] private Button _restartGameBtn;

    [SerializeField] private Transform _panelGameDoneObj;
    [SerializeField] private Text _scoreResultTxt;
    [SerializeField] private Text _levelResultTxt;

    public event Action OnGameStarted;

    private void Awake()
    {
        if (_panelGameOverObj != null)
        {
            _panelGameOverObj.gameObject.SetActive(false);
        }
        if (_panelGameDoneObj != null)
        {
            _panelGameDoneObj.gameObject.SetActive(false);
        }
        if (_restartGameBtn != null)
        {
            _restartGameBtn.onClick.AddListener(GameStart);
        }
    }

    private void Start()
    {
        UpdateLevel(GameManager.Instance.Level);
        UpdateScore(GameManager.Instance.Coin);
    }

    public void GameStart()
    {
        OnGameStarted?.Invoke();
    }

    public void UpdateLevel(byte level)
    {
        _levelTxt.text = level.ToString();
        if (_levelResultTxt != null) _levelResultTxt.text = level.ToString();
    }

    public void UpdateScore(int score)
    {
        _scoreTxt.text = score.ToString();
        if (_scoreResultTxt != null) _scoreResultTxt.text = score.ToString();
        Debug.Log("_scoreManager.UpdateScore: " + score);
    }

    public void HideScore()
    {
        _levelTxt.transform.parent.gameObject.SetActive(false);
        _scoreTxt.transform.parent.gameObject.SetActive(false);
    }

    public void GameDone()
    {
        HideScore();
        if (_panelGameDoneObj != null)
        {
            _panelGameDoneObj.gameObject.SetActive(true);
            if (_panelGameDoneObj.childCount > 0) _panelGameDoneObj.GetChild(0).DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.45f, 5);
        }
    }

    public void GameOver()
    {
        HideScore();
        if (_panelGameOverObj != null)
        {
            _panelGameOverObj.gameObject.SetActive(true);
            if (_panelGameOverObj.childCount > 0) _panelGameOverObj.GetChild(0).DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.45f, 5);
        }
    }
}

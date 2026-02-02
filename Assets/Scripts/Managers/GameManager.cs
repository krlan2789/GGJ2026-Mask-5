using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            _instance = _instance != null ? _instance : FindFirstObjectByType<GameManager>();
            return _instance;
        }
    }

    private GameController _gameController;
    private SfxManager _sfxManager;
    private PlayerMovement _playerMovement;
    private ScoreManager _scoreManager;
    private CoinManager _coinManager;
    private LevelManager _levelManager;

    private bool _isGameOver = false;
    public bool IsGameOver => _isGameOver;
    public byte Level => _levelManager.Level;
    public int Coin => _coinManager.CoinValue;

    public void OnValidate()
    {
        _gameController = FindFirstObjectByType<GameController>();
        _sfxManager = FindFirstObjectByType<SfxManager>();
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        _scoreManager = FindFirstObjectByType<ScoreManager>();
        _coinManager = FindFirstObjectByType<CoinManager>();

        if (_gameController == null)
            Debug.LogError("GameController not found in scene!");
        if (_sfxManager == null)
            Debug.LogError("SfxManager not found in scene!");
        if (_playerMovement == null)
            Debug.LogError("PlayerMovement not found in scene!");
        if (_scoreManager == null)
            Debug.LogError("ScoreManager not found in scene!");
        if (_coinManager == null)
            Debug.LogError("CoinManager not found in scene!");
    }

    private void Awake()
    {
        _levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void Start()
    {

        Debug.Log("GameManager -> LoadLevelStage");
        _levelManager.LevelUp();
        _levelManager.LoadLevelStage();

        OnValidate();

        if (_gameController != null && _playerMovement != null)
        {
            _gameController.OnMove += _playerMovement.Move;
            _gameController.OnJump += _playerMovement.Jump;
            _gameController.OnHide += _playerMovement.Hiding;
        }

        if (_gameController != null && _sfxManager != null)
        {
            _gameController.OnMove += _sfxManager.PlayWalkSound;
            _gameController.OnJump += _sfxManager.PlayJumpSound;
        }

        if (_coinManager != null && _scoreManager != null)
        {
            _coinManager.OnCoinUpdated += _scoreManager.UpdateScore;
            Debug.Log("_scoreManager.UpdateScore registered!");
        }

        if (_levelManager != null && _scoreManager != null)
        {
            _levelManager.OnLevelChanged += _scoreManager.UpdateLevel;
            Debug.Log("_scoreManager.UpdateLevel registered!");
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnGameStarted += ResetGame;
        }
    }

    private void OnDestroy()
    {
        if (_gameController != null && _playerMovement != null)
        {
            _gameController.OnMove -= _playerMovement.Move;
            _gameController.OnJump -= _playerMovement.Jump;
        }

        if (_gameController != null && _sfxManager != null)
        {
            _gameController.OnMove -= _sfxManager.PlayWalkSound;
            _gameController.OnJump -= _sfxManager.PlayJumpSound;
        }

        if (_coinManager != null && _scoreManager != null)
        {
            _coinManager.OnCoinUpdated -= _scoreManager.UpdateScore;
        }

        if (_levelManager != null && _scoreManager != null)
        {
            _levelManager.OnLevelChanged -= _scoreManager.UpdateLevel;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnGameStarted -= ResetGame;
        }
    }

    public void LevelUp()
    {
        if (_levelManager.Level < _levelManager.MaxLevel)
        {
            SceneLoader.LoadSceneStatic(SceneLoader.SceneEnum.Gameplay);
        }

        if (_levelManager.Level >= _levelManager.MaxLevel)
        {
            _scoreManager.GameDone();
        }
    }

    //public void LevelDown()
    //{
    //    if (_levelManager.Level > 1) _levelManager.LevelDown();
    //    SceneLoader.LoadSceneStatic(SceneLoader.SceneEnum.Gameplay);
    //}

    public void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        Debug.Log("Game Over!");
        _scoreManager.GameOver();
    }

    public void ResetGame()
    {
        if (_isGameOver) _coinManager.ResetScore();
        SceneLoader.LoadSceneStatic(SceneLoader.SceneEnum.Gameplay);
        Debug.Log("Game Reset!");
    }
}
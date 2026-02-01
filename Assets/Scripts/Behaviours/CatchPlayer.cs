using UnityEngine;

public class CatchPlayer : MonoBehaviour
{
    private GameManager _gameManager;
    private EnemyMovement _enemyMovement;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _enemyMovement = GetComponentInParent<EnemyMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ConstantHelper.Tags.PLAYER))
        {
            var isHiding = collision.gameObject.GetComponent<PlayerMovement>().IsHiding;
            if (isHiding) return;
            if (_gameManager != null) _gameManager.GameOver();
            if (_enemyMovement != null) _enemyMovement.StopMovement();
        }
    }
}

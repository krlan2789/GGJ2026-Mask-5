using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _attackRange = 1.2f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private int _maxHealth = 3;

    [Header("Patrol")]
    [SerializeField] private float _patrolDistance = 5f;
    [SerializeField] private float _patrolChangeDelay = 2f;

    [Header("Optional")]
    [SerializeField] private Animator _animator;

    private Transform _player;
    private PlayerMovement _playerMovement;
    private Rigidbody2D _rb;
    private float _attackTimer;
    private int _currentHealth;

    private Vector2 _startPosition;
    private float _patrolLeftBound;
    private float _patrolRightBound;
    private int _patrolDirection = 1; // 1 for right, -1 for left
    private float _patrolChangeTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = _maxHealth;
        _startPosition = transform.position;
        _patrolLeftBound = _startPosition.x - _patrolDistance;
        _patrolRightBound = _startPosition.x + _patrolDistance;
        _patrolChangeTimer = _patrolChangeDelay;
    }

    /// <summary>
    /// Called by spawner to provide the player reference.
    /// </summary>
    public void Initialize(Transform player)
    {
        _player = player;
        _playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _attackTimer = 0f;
        _currentHealth = _maxHealth;
        _patrolChangeTimer = _patrolChangeDelay;
    }

    private void Update()
    {
        if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

        if (_player == null)
        {
            var found = GameObject.FindWithTag(ConstantHelper.Tags.PLAYER);
            if (found != null)
            {
                _player = found.transform;
                _playerMovement = _player.GetComponent<PlayerMovement>();
            }
        }
    }

    private void FixedUpdate()
    {
        //if (_player == null) return;

        //Vector2 toPlayer = _player.position - transform.position;
        //float dist = toPlayer.magnitude;

        //// If player is hiding, patrol instead of chasing
        //if (_playerMovement != null && _playerMovement.IsHiding)
        //{
        //    Patrol();
        //    return;
        //}

        //if (dist <= _attackRange)
        //{
        //    // Attack
        //    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        //    TryAttack();
        //    if (_animator != null) _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, false);
        //} else if (dist <= _detectionRange)
        //{
        //    // Chase player horizontally (preserve vertical velocity)
        //    float dir = Mathf.Sign(toPlayer.x);
        //    _rb.linearVelocity = new Vector2(dir * _moveSpeed, _rb.linearVelocity.y);

        //    // flip to face player
        //    Vector3 ls = transform.localScale;
        //    ls.x = Mathf.Abs(ls.x) * dir;
        //    transform.localScale = ls;

        //    if (_animator != null) _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, true);
        //} else
        //{
        //    // Patrol
        //}
        Patrol();
    }

    private void Patrol()
    {
        // Move in patrol direction
        _rb.linearVelocity = new Vector2(_patrolDirection * _moveSpeed, _rb.linearVelocity.y);

        // Update sprite direction
        Vector3 ls = transform.localScale;
        ls.x = Mathf.Abs(ls.x) * _patrolDirection;
        transform.localScale = ls;

        // Check if reached patrol boundary
        if ((_patrolDirection > 0 && transform.position.x >= _patrolRightBound) ||
            (_patrolDirection < 0 && transform.position.x <= _patrolLeftBound))
        {
            _patrolDirection *= -1; // Change direction
            _patrolChangeTimer = _patrolChangeDelay;
        }

        if (_animator != null) _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, true);
    }

    private void TryAttack()
    {
        if (_attackTimer > 0f) return;

        // Trigger animator (if any)
        if (_animator != null) _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_ATTACK);

        // Attempt to apply damage to player. Player should implement a method named "ApplyDamage(int)" or accept SendMessage.
        if (_player != null)
        {
            _player.gameObject.SendMessage(nameof(Damagable.TakeDamage), _damage, SendMessageOptions.DontRequireReceiver);
        }

        _attackTimer = _attackCooldown;
    }

    /// <summary>
    /// Apply damage to enemy. If health <= 0 the enemy is deactivated (so spawner can reuse it).
    /// </summary>
    public void ApplyDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void StopMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _moveSpeed = 0;
    }

    private void Die()
    {
        // play death animation or effects if animator present
        if (_animator != null)
        {
            _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_DIE);
            // disable movement right away; animation can disable object by calling a timeline event or automatically
            _rb.linearVelocity = Vector2.zero;
        } else
        {
            // If no animator, immediately deactivate for pooling
            gameObject.SetActive(false);
        }
    }

    // Optional: called by an animation event at the end of death animation
    public void OnDeathAnimationEnd()
    {
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        // Draw patrol bounds
        Vector2 startPos = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        float leftBound = startPos.x - _patrolDistance;
        float rightBound = startPos.x + _patrolDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(leftBound, startPos.y - 0.5f, 0), new Vector3(leftBound, startPos.y + 0.5f, 0));
        Gizmos.DrawLine(new Vector3(rightBound, startPos.y - 0.5f, 0), new Vector3(rightBound, startPos.y + 0.5f, 0));
        Gizmos.DrawLine(new Vector3(leftBound, startPos.y, 0), new Vector3(rightBound, startPos.y, 0));
    }
}
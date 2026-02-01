using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Damagable : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private bool _destroyOnDeath = false;

    [Header("Invulnerability")]
    [Tooltip("Seconds of invulnerability after taking damage.")]
    [SerializeField] private float _invulnerabilityTime = 0.5f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    [Header("Events")]
    [SerializeField] private UnityEvent<int> _onDamaged;
    [SerializeField] private UnityEvent _onDied;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvulnerable => _isInvulnerable;

    private bool _isInvulnerable;
    private float _invTimer;

    public event System.Action<int> OnDamaged;
    public event System.Action OnDied;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    private void OnEnable()
    {
        // reset invulnerability and ensure health is valid when reused by pooling
        if (CurrentHealth <= 0) CurrentHealth = _maxHealth;
        _isInvulnerable = false;
        _invTimer = 0f;
    }

    private void Update()
    {
        if (_isInvulnerable)
        {
            _invTimer -= Time.deltaTime;
            if (_invTimer <= 0f) _isInvulnerable = false;
        }
    }

    /// <summary>
    /// Main damage entry point.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (!IsAlive) return;
        if (_isInvulnerable) return;

        CurrentHealth -= damage;

        // Notify listeners
        OnDamaged?.Invoke(damage);
        _onDamaged?.Invoke(damage);

        if (_animator != null)
            _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_HIT);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        StartInvulnerability();
    }

    public void SetMaxHealth(int max, bool healToFull = true)
    {
        _maxHealth = Mathf.Max(1, max);
        if (healToFull) CurrentHealth = _maxHealth;
        else CurrentHealth = Mathf.Min(CurrentHealth, _maxHealth);
    }

    private void StartInvulnerability()
    {
        if (_invulnerabilityTime > 0f)
        {
            _isInvulnerable = true;
            _invTimer = _invulnerabilityTime;
        }
    }

    private void Die()
    {
        OnDied?.Invoke();
        _onDied?.Invoke();

        if (_animator != null)
        {
            _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_DIE);
            // movement/physics should be stopped by the animator event or elsewhere.
        } else
        {
            // immediate fallback for pooling or simple objects
            if (_destroyOnDeath) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called from a death animation event to finalize deactivation when using animations.
    /// </summary>
    public void OnDeathAnimationEnd()
    {
        if (_destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
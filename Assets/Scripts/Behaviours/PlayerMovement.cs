using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private HintManager _hintManager;

    [SerializeField] private float _moveSpeed = 5f;
    public float MovementSpeed => _moveSpeed * Multiplier;
    
    [SerializeField] private float _multiplier = 2f;
    public bool IsRunning => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    public float Multiplier
    {
        get
        {
            var v = IsRunning ? _multiplier : 1;
            return v;
        }
    }

    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _jumpCooldown = 0.5f;
    public bool PreventToJump { private set; get; } = false;
    private float _lastJumpTime = -Mathf.Infinity;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private BoundaryManager _boundaryManager;

    private float _gravityScaleOriginal = 2;
    private Vector2 _lastMoveDirection = Vector2.zero;
    public bool IsHiding { private set; get; } = false;

    public bool PerformToHiding { private set; get; } = false;
    public bool PerformToTeleport { private set; get; } = false;

    private void OnValidate()
    {
        if (_boundaryManager == null) _boundaryManager = FindFirstObjectByType<BoundaryManager>();
        //if (_boundaryManager == null)
        //    Debug.LogError("BoundaryManager not found in scene!");
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        OnValidate();
    }

    public void Move(Vector2 direction)
    {
        if (IsHiding) return;
        Vector2 targetPosition = (Vector2)transform.position + direction;
        Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, MovementSpeed * Time.deltaTime);
        
        OnValidate();
        // Constrain to boundaries
        if (_boundaryManager != null)
        {
            newPosition = _boundaryManager.ConstrainPosition(newPosition);
        }

        transform.position = newPosition;
        _lastMoveDirection = direction;
        UpdateMovementAnimation(direction);
    }

    public void Jump()
    {
        if (PerformToTeleport)
        {
            GameManager.Instance.LevelUp();
            return;
        }
        if (IsHiding)
        {
            StopHiding();
            return;
        }

        if (Time.time - _lastJumpTime >= _jumpCooldown && !PreventToJump)
        {
            _lastJumpTime = Time.time;
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            if (_animator != null)
            {
                _animator.SetTrigger(ConstantHelper.Combat.ANIMATOR_PARAM_JUMP);
                Debug.Log("Jump performed!");
            }
        }
    }

    public void Hiding()
    {
        if (PerformToTeleport)
        {
            GameManager.Instance.LevelUp();
            return;
        }
        Debug.Log($"Attempting to hide... {PerformToHiding} - {!IsHiding}");
        if (PerformToHiding && !IsHiding)
        {
            IsHiding = true;
            PreventToJump = true;
            _spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            _gravityScaleOriginal = _rb.gravityScale;
            _rb.gravityScale = 0f;
            _collider.enabled = false;

            // Play hiding animation
            if (_animator != null)
            {
                _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_HIDE, true);
                _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, false);
                _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_RUN, false);
            }

            Debug.Log("Player is hiding!");
        }
    }

    public void StopHiding()
    {
        if (IsHiding)
        {
            IsHiding = false;
            PreventToJump = false;
            _spriteRenderer.color = Color.white;
            _collider.enabled = true;
            _rb.gravityScale = _gravityScaleOriginal;

            // Stop hiding animation
            if (_animator != null)
            {
                _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_HIDE, false);
            }
        }
    }

    private void UpdateMovementAnimation(Vector2 direction)
    {
        if (_animator == null)
            return;

        // Only update movement animation if not hiding
        if (!IsHiding)
        {
            bool isWalking = direction.magnitude > 0.1f;
            _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, isWalking);
            _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_RUN, IsRunning);

            // Flip sprite based on direction
            if (direction.x > 0.1f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            } else if (direction.x < -0.1f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.PORTAL))
        {
            var floor = collision.gameObject.GetComponent<FloorManager>();
            if (floor.IsExit) _hintManager.ShowHint("Double tap S or Down Arrow to Next Floor");
            PerformToTeleport = true;
        } else
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.HIDABLE))
        {
            _hintManager.ShowHint("Double tap S or Down Arrow to Hide");
            PreventToJump = true;
            PerformToHiding = true;
        } else
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.GROUND) ||
            collision.gameObject.CompareTag(ConstantHelper.Tags.OBSTACLE))
        {
            PreventToJump = false;
        }
        Debug.Log($"Collided with: {collision.gameObject.name}");
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.HIDABLE))
        {
            _hintManager.HideHint();
            PreventToJump = false;
            PerformToHiding = false;
        } else
        if (collision.gameObject.CompareTag(ConstantHelper.Tags.PORTAL))
        {
            _hintManager.HideHint();
            PerformToTeleport = false;
        }
    }
}
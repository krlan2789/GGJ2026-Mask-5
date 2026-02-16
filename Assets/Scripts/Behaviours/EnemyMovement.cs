using GGJ_2026_Mask_5.Values;
using UnityEngine;

namespace GGJ_2026_Mask_5.Behaviours
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _detectionRange = 8f;
        [SerializeField] private float _attackRange = 1.2f;

        [Header("Patrol")]
        [SerializeField] private float _patrolDistance = 5f;
        [SerializeField] private float _patrolChangeDelay = 2f;

        [Header("Optional")]
        [SerializeField] private Animator _animator;

        private Transform _player;
        private Rigidbody2D _rb;

        private Vector2 _startPosition;
        private float _patrolLeftBound;
        private float _patrolRightBound;
        private int _patrolDirection = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _startPosition = transform.position;
            _patrolLeftBound = _startPosition.x - _patrolDistance;
            _patrolRightBound = _startPosition.x + _patrolDistance;
        }

        /// <summary>
        /// Called by spawner to provide the player reference.
        /// </summary>
        public void Initialize(Transform player)
        {
            _player = player;
        }

        private void Update()
        {
            if (_player == null)
            {
                var found = GameObject.FindWithTag(ConstantHelper.Tags.PLAYER);
                if (found != null)
                {
                    _player = found.transform;
                }
            }
        }

        private void FixedUpdate()
        {
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
                _patrolDirection *= -1;
            }

            if (_animator != null) _animator.SetBool(ConstantHelper.Combat.ANIMATOR_PARAM_WALK, true);
        }

        public void StopMovement()
        {
            _rb.linearVelocity = Vector2.zero;
            _moveSpeed = 0;
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
}
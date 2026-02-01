using System;
using UnityEngine;

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    public Action<Vector2> OnMove;
    public Action OnJump;
    public Action OnHide;

    [SerializeField] private float _doubleTapWindow = 0.3f;

    private float _lastDownTapTime = -Mathf.Infinity;
    private bool _isAwaitingSecondTap = false;

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (gameObject == null) return;
        // Movement
        var horizontalInput = Input.GetAxis("Horizontal");
        Move(new Vector2(horizontalInput, 0));

        // Jump
        Jump();

        // Hiding
        Hide();

        // Double tap detection
        DetectDoubleDownTap();
    }

    private void Move(Vector2 direction)
    {
        OnMove?.Invoke(direction);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnJump?.Invoke();
        }
    }

    private void Hide()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnHide?.Invoke();
        }
    }

    private void DetectDoubleDownTap()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            float timeSinceLastTap = Time.time - _lastDownTapTime;

            // Check if within double-tap window
            if (timeSinceLastTap <= _doubleTapWindow && _isAwaitingSecondTap)
            {
                // Double-tap detected!
                OnHide?.Invoke();
                _isAwaitingSecondTap = false;
                _lastDownTapTime = -Mathf.Infinity;
            } else
            {
                // First tap or too slow
                _lastDownTapTime = Time.time;
                _isAwaitingSecondTap = true;
            }
        }

        // Reset if window has passed without second tap
        if (_isAwaitingSecondTap && Time.time - _lastDownTapTime > _doubleTapWindow)
        {
            _isAwaitingSecondTap = false;
        }
    }
}

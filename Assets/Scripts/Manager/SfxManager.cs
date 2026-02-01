using UnityEngine;

[DisallowMultipleComponent]
public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _jumpClip;
    [SerializeField] private AudioClip _fallClip;
    [SerializeField] private AudioClip _claimArtifactClip;
    [SerializeField] private AudioClip _transition1Clip;
    [SerializeField] private AudioClip _transition2Clip;
    [SerializeField] private AudioClip _gameOverClip;
    
    private bool _isWalkSoundPlaying = false;

    private void Awake()
    {
        _audioSource = FindFirstObjectByType<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.volume = 0.5f;
    }

    public void PlayWalkSound(Vector2 v)
    {
        if (_audioSource == null || _walkClip == null) return;

        // Check if movement is active
        bool isMoving = v.magnitude > 0.1f;

        if (isMoving)
        {
            // Start walk sound if not already playing
            if (!_isWalkSoundPlaying)
            {
                _audioSource.clip = _walkClip;
                _audioSource.loop = true;
                _audioSource.Play();
                _isWalkSoundPlaying = true;
            }
        } else
        {
            // Stop walk sound if player stopped moving
            if (_isWalkSoundPlaying)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
                _isWalkSoundPlaying = false;
            }
        }
    }

    public void PlayJumpSound()
    {
        if (_jumpClip) _audioSource.PlayOneShot(_jumpClip);
    }

    public void PlayFallSound()
    {
        if (_fallClip) _audioSource.PlayOneShot(_fallClip);
    }

    public void PlayClaimArtifactSound()
    {
        if (_claimArtifactClip) _audioSource.PlayOneShot(_claimArtifactClip);
    }

    public void PlayTransitionSound(byte num)
    {
        if (num == 1 && _transition1Clip) _audioSource.PlayOneShot(_transition1Clip);
        if (num == 2 && _transition2Clip) _audioSource.PlayOneShot(_transition2Clip);
    }

    public void PlayGameOverSound()
    {
        if (_gameOverClip) _audioSource.PlayOneShot(_gameOverClip);
    }
}
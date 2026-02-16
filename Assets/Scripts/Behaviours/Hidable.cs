using GGJ_2026_Mask_5.Values;
using UnityEngine;

namespace GGJ_2026_Mask_5.Behaviours
{
    [DisallowMultipleComponent]
    public class Hidable : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _closeSprite;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_openSprite != null) _spriteRenderer.sprite = _openSprite;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConstantHelper.Tags.PLAYER))
            {
                //collision.gameObject.GetComponent<PlayerMovement>().PerformToHiding = true;
                if (_spriteRenderer != null && _closeSprite != null)
                {
                    _spriteRenderer.sprite = _closeSprite;
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConstantHelper.Tags.PLAYER))
            {
                //collision.gameObject.GetComponent<PlayerMovement>().PerformToHiding = false;
                if (_spriteRenderer != null && _openSprite != null)
                {
                    _spriteRenderer.sprite = _openSprite;
                }
            }
        }
    }
}

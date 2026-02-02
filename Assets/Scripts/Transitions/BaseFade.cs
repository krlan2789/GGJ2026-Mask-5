using UnityEngine;
using UnityEngine.Events;

namespace GGJ_2026_Mask_5.Transitions
{
    public abstract class BaseFade : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup[] _canvasGroups;
        [SerializeField] protected float _timing = 1.5f;
        [SerializeField] protected float _delay = 0.5f;
        [SerializeField] protected float _delayBetween = 0.5f;

        public bool autoStart = true;
        public UnityEvent OnComplete;

        public abstract void StartTransition();
    }
}

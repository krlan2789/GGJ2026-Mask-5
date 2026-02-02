using UnityEngine;
using UnityEngine.Events;

namespace GGJ_2026_Mask_5.Transitions
{
    public abstract class BaseFade : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup[] _canvasGroups;
        [SerializeField] protected float _timing = 1.5f;
        [SerializeField] protected float _delay = 0f;
        [SerializeField] protected float _delayBetween = 0f;

        public bool autoStart = true;
        public UnityEvent OnComplete;

        public abstract void StartTransition();

        protected void StartWith0Alpha()
        {
            foreach (var cg in _canvasGroups)
            {
                cg.alpha = 0;
            }
        }

        protected void StartWith1Alpha()
        {
            foreach (var cg in _canvasGroups)
            {
                cg.alpha = 1;
            }
        }
    }
}

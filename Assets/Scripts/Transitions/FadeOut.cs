using System.Collections;
using UnityEngine;

namespace GGJ_2026_Mask_5.Transitions
{
    public class FadeOut : BaseFade
    {
        private void Start()
        {
            if (autoStart) StartTransition();
        }

        public override void StartTransition()
        {
            StartTransition(_canvasGroups, _timing, _delayBetween);
        }

        public void StartTransition(CanvasGroup[] cg, float timing = 1.5f, float delayBetween = 0.5f)
        {
            StartCoroutine(StartTransitioning(cg, timing, delayBetween));
        }

        private IEnumerator StartTransitioning(CanvasGroup[] cg, float timing, float delayBetween)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            for (int a = 0; a < cg.Length; a++)
            {
                cg[a].alpha = 1;
                while (cg[a].alpha > 0)
                {
                    cg[a].alpha -= Time.deltaTime / timing;
                    if (Input.GetMouseButtonDown(0))
                    {
                        cg[a].alpha = 0;
                    }
                    yield return null;
                }

                if (a < cg.Length - 1) yield return new WaitForSeconds(delayBetween);
            }

            OnComplete?.Invoke();
        }
    }
}

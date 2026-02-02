using UnityEngine;

namespace GGJ_2026_Mask_5
{
    public class Splashscreen : MonoBehaviour
    {
        [SerializeField] private SceneLoader.SceneEnum _sceneTarget;

        public void LoadNextScene()
        {
            SceneLoader.LoadSceneStatic(_sceneTarget);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SceneLoader : MonoBehaviour
{
    public enum SceneEnum
    {
        Menu,
        Gameplay,
        GameOver,
        Credit,
        Exit
    }

    [SerializeField] private Button button;
    [SerializeField] private SceneEnum scene;

    private void OnValidate()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void Start()
    {
        if (button != null) button.onClick.AddListener(() => LoadScene(scene));
    }

    public void LoadScene(SceneEnum scene)
    {
        LoadSceneStatic(scene);
    }

    public static void LoadSceneStatic(SceneEnum scene)
    {
        if (scene == SceneEnum.Exit)
        {
            Application.Quit();
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}

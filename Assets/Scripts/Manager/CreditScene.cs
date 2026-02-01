using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditScene : MonoBehaviour
{
    [SerializeField] private Image[] panels;

    private IEnumerator Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            yield return new WaitForSeconds(1.5f);

            if (i >= panels.Length - 1)
            {
                yield return new WaitForSeconds(3f);
                SceneLoader.LoadSceneStatic(SceneLoader.SceneEnum.Menu);
                yield break;
            } else
            {
                while (panels[i].color.a > 0f)
                {
                    Color panelColor = panels[i].color;
                    panelColor.a -= Time.deltaTime;
                    panels[i].color = panelColor;
                    yield return new WaitForSeconds(0.0001f);
                }
            }
        }
    }
}

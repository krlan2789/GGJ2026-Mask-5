using UnityEngine;
using UnityEngine.UI;

namespace GGJ_2026_Mask_5
{
    public class OpenUrl : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private string _url;

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(() => Open(_url));
            }
        }

        public void Open(string url)
        {
            Application.OpenURL(url);
        }
    }
}

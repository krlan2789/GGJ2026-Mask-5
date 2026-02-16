using UnityEngine;
using UnityEngine.UI;

namespace GGJ_2026_Mask_5.Managers
{
    [DisallowMultipleComponent]
    public class HintManager : MonoBehaviour
    {
        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private Text _hintMessage;
    
        private void Start()
        {
            _hintPanel.SetActive(false);
        }

        public void ShowHint(string hintMessage)
        {
            _hintPanel.SetActive(true);
            if (_hintMessage != null)
            {
                _hintMessage.text = hintMessage;
            }
        }

        public void HideHint()
        {
            _hintPanel.SetActive(false);
        }
    }
}

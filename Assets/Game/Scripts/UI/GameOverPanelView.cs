using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class GameOverPanelView : MonoBehaviour
    {
        [Header("UI refs")] 
        [SerializeField] private GameObject container;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button continueButton;
        
        private Action _onClose;
        
        public void Initialize(bool success, Action onClose = null)
        {
            titleText.text = success ? "Level Completed!" : "Level Failed!";
            
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            _onClose = onClose; 
            Open();
        }

        public void Dispose()
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            Close();
        }
        
        public void Open()
        {
            container.SetActive(true);
        }

        public void Close()
        {
            container.SetActive(false);
            
            _onClose?.Invoke();
        }

        private void OnContinueButtonClicked()
        {
            Dispose();
        }
    }
}

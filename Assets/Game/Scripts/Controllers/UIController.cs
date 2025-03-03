using System;
using Game.Scripts.Helpers;
using Game.Scripts.UI;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class UIController : Singleton<UIController>
    {
        [SerializeField] private LevelIndicatorElement levelIndicatorElement;
        [SerializeField] private GameOverPanelView gameOverPanelView;
        
        public void Initialize()
        {
            
        }
    
        public void Dispose()
        {
            
        }
        
        public void OpenGameOverPanel(bool isSuccess, Action onClose = null)
        {
            gameOverPanelView.Initialize(isSuccess, onClose);
        }
        
        public void UpdateLevel(int levelIndex)
        {
            levelIndicatorElement.UpdateLevel(levelIndex);
        }
    }
}

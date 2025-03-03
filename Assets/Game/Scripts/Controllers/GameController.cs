using System;
using System.Collections.Generic;
using Game.Scripts.Data;
using Game.Scripts.Helpers;
using Game.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Controllers
{
    public class GameController : Singleton<GameController>
    {
        private ICharacterController _playerCharacterController;
        private IStackController _stackController;
        
        [Header("Level Configurations")]
        // List of level configurations (assign in the Inspector)
        public List<LevelData> levelDataList;
        
        public static event Action GameStarted;
        public static event Action<bool> GameEnded;
      
        public int CompletedLevelCount
        {
            get => PlayerPrefs.GetInt(nameof(CompletedLevelCount), 0);
            set => PlayerPrefs.SetInt(nameof(CompletedLevelCount), value);
        }
        
        // Zenject will inject the dependency.
        [Inject]
        public void Construct(ICharacterController playerCharacterController, IStackController stackController)
        {
            _playerCharacterController = playerCharacterController;
            _stackController = stackController;
        }
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Initialize()
        {
           UIController.Instance.Initialize();

           _playerCharacterController.OnFellFromPlatform+= OnFellFromPlatform;
           _playerCharacterController.OnReachedToFinish += OnPlayerCharacterOnReachedFinalPlatform;
           
            LoadGame();
            StartGame();

        }
    
        private void Dispose()
        {
            UIController.Instance.Dispose();
            
            _playerCharacterController.OnFellFromPlatform-= OnFellFromPlatform;
            _playerCharacterController.OnReachedToFinish -= OnPlayerCharacterOnReachedFinalPlatform;

            _playerCharacterController.Dispose();
            
            _stackController.Dispose();
        }

        private void OnFellFromPlatform()
        {
            OnGameEnded(false);
        }

        private void OnPlayerCharacterOnReachedFinalPlatform()
        {
            CameraController.Instance.ActivateFinishVCam(_playerCharacterController.Transform);

            OnGameEnded(true);
        }

        private void LoadGame()
        {
            _stackController.Initialize(GetCurrentLevelData());

            var playerStartPoint = _stackController.AnchorPlatformBounds;

            _playerCharacterController.Transform.position =
                new Vector3(playerStartPoint.center.x, 0, playerStartPoint.center.z);
            
            _playerCharacterController.Initialize();
            UIController.Instance.UpdateLevel(CompletedLevelCount);
        }
        
        private void UnLoadGame()
        {
            _stackController.Dispose();
            _playerCharacterController.Dispose();
        }

        private void StartGame()
        {
            GameStarted?.Invoke();
            
            CameraController.Instance.ActivateFollowVCam(_playerCharacterController.Transform);
        }

        private void OnGameEnded(bool success)
        {
            GameEnded?.Invoke(success);

            if (success)
            {
                CompletedLevelCount++;
            }
            
            //open game over screen
            UIController.Instance.OpenGameOverPanel(success, OnGameOverPanelClosed);
        }

        private void OnGameOverPanelClosed()
        {
            UnLoadGame();
            LoadGame();

            StartGame();
        }

        /// <summary>
        /// Loads the current level data based on the player's completed level count.
        /// </summary>
        private LevelData GetCurrentLevelData()
        {
            // Retrieve the completed level count from PlayerPrefs (default to 0)
            int completedLevelCount = PlayerPrefs.GetInt("CompletedLevelCount", 0);
        
            // Determine the current level index.
            // Use modulo in case completedLevelCount exceeds the list count (cycling through levels).
            int levelIndex = completedLevelCount % levelDataList.Count;
        
            var currentLevelData = levelDataList[levelIndex];
            Debug.Log("Loaded Level Index: " + levelIndex);

            return currentLevelData;
        }

    }
}

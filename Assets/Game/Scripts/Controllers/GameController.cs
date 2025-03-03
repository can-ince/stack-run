using System;
using System.Collections.Generic;
using Game.Scripts.Data;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class GameController : Singleton<GameController>
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        
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
           
           characterController.OnFellFromPlatform+= OnFellFromPlatform;
           characterController.OnReachedToFinish += OnCharacterOnReachedFinalPlatform;
           characterController.Initialize();
           
           StackController.Instance.Initialize(GetCurrentLevelData());
           StartGame();

        }
    
        private void Dispose()
        {
            characterController.OnFellFromPlatform-= OnFellFromPlatform;
            characterController.OnReachedToFinish -= OnCharacterOnReachedFinalPlatform;

            characterController.Dispose();
            
            StackController.Instance.Dispose();
        }

        private void OnFellFromPlatform()
        {
            OnGameEnded(false);
        }

        private void OnCharacterOnReachedFinalPlatform()
        {
            CameraController.Instance.ActivateFinishVCam(characterController.transform);

            OnGameEnded(true);
        }

        private void StartGame()
        {
            GameStarted?.Invoke();
            
            CameraController.Instance.ActivateFollowVCam(characterController.transform);
        }

        private void OnGameEnded(bool success)
        {
            GameEnded?.Invoke(success);

            if (success)
            {
                CompletedLevelCount++;
            }
            //todo: open game over screen
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

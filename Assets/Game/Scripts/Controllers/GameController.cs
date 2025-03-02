using System;
using Game.Scripts.Helpers;
using Game.Scripts.Interfaces;

namespace Game.Scripts.Controllers
{
    public class GameController : Singleton<GameController>
    {
        public CharacterController characterController;
        public static event Action GameStarted;
        public static event Action<bool> GameEnded;
        
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
           
           characterController.OnCharacterFell+= OnCharacterFell;
           
           characterController.Initialize();
           
           StartGame();

        }
    
        private void Dispose()
        {
            characterController.OnCharacterFell-= OnCharacterFell;
            
            characterController.Dispose();
        }

        private void OnCharacterFell()
        {
            OnGameEnded(false);
        }

        private void OnCharacterReachedFinalPlatform()
        {
            OnGameEnded(true);
        }

        private void StartGame()
        {
            GameStarted?.Invoke();
        }

        private void OnGameEnded(bool success)
        {
            GameEnded?.Invoke(success);
            //todo: open game over screen
        }
    }
}

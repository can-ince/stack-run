using Game.Scripts.Helpers;

namespace Game.Scripts.Controllers
{
    public class GameController : Singleton<GameController>
    {
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
           
        }
    
        private void Dispose()
        {
            
        }

        private void OnGameEnded(bool success)
        {
            
        }
    }
}

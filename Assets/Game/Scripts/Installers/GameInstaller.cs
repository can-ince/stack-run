using Game.Scripts.Behaviours;
using Game.Scripts.Controllers;
using Game.Scripts.Interfaces;
using Zenject;

namespace Game.Scripts.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public StackPlatformBehaviour stackPlatformPrefab;
        public PlayerCharacterController characterController;
        public AudioController audioController;
        public StackController stackController;
        public GameController gameController;
        
        public override void InstallBindings()
        {
            // Platform and stack controls
            Container.Bind<IStackPlatform>().To<StackPlatformBehaviour>().FromComponentInNewPrefab(stackPlatformPrefab).
                UnderTransformGroup("Platforms").AsTransient();
            Container.Bind<IStackController>().To<StackController>().FromInstance(stackController).AsSingle();

            // Character controls
            Container.Bind<ICharacterController>().To<PlayerCharacterController>().FromInstance(characterController).AsSingle();

            // Audio controls
            Container.Bind<IAudioController>().To<AudioController>().FromInstance(audioController).AsSingle();

            // Game controls
            Container.Bind<GameController>().FromInstance(gameController).AsSingle();
        }
    }
}

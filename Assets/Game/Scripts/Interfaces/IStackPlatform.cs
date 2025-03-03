using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface IStackPlatform
    {
        void StartMoving();
        void StopMoving();
        bool TryCutStackPlatform(IStackPlatform previousPlatform);
        public Collider Collider { get; }
        
        public GameObject GameObject { get; }
        
        public Rigidbody Rigidbody { get; }
        
        public Bounds Bounds { get; }
    }
}

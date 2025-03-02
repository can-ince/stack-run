using System;
using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface IStackPlatform
    {
        void StartMoving();
        void StopMoving();
        void CutStackPlatform(IStackPlatform previousPlatform);
        event Action OnPlatformStopped;
        public Collider Collider { get; }
        
        public GameObject GameObject { get; }
        
        public Rigidbody Rigidbody { get; }
    }
}

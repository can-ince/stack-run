using System;
using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface IStackPlatform
    {
        void Initialize(IStackPlatform previousPlatform, Vector3 moveDir, float moveSpeed, Material colorMat);
        void Dispose();
        void StartMoving();
        void StopMoving();
        bool TryCutStackPlatform(IStackPlatform previousPlatform);
        public Collider Collider { get; }
        
        public GameObject GameObject { get; }
        
        public Rigidbody Rigidbody { get; }
        
        public Bounds Bounds { get; }
        
        event Action<IStackPlatform> PlatformDriftedAway;
    }
}

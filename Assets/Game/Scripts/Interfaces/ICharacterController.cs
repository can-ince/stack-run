using System;
using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface ICharacterController 
    {
        void MoveToPlatform(Vector3 platformCenter);
        void PlayCelebration();
        void Fall();
        
        void Initialize();
        
        void Dispose();
        
        event Action OnFellFromPlatform;
        event Action OnReachedToFinish;
        
        Transform Transform { get; }

    }
}

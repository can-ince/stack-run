using System;
using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface ICharacterController 
    {
        void MoveToPlatform(Vector3 platformCenter);
        void PlayCelebration();
        void Fall();
        
        event Action OnCharacterFell;

    }
}

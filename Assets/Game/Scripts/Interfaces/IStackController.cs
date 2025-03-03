using System;
using Game.Scripts.Data;
using UnityEngine;

namespace Game.Scripts.Interfaces
{
    public interface IStackController 
    {
        void Initialize(LevelData levelData);
        void Dispose();

        Bounds AnchorPlatformBounds { get; }
        
        event Action<IStackPlatform> StackingFailed; 
        event Action<IStackPlatform> StackingSucceed;
    }
}

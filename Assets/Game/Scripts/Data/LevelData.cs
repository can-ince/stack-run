using UnityEngine;

namespace Game.Scripts.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Platform Stack Settings")]
        [Tooltip("The number of platforms to be stacked.")]
        public int stackCount = 10;
        
    }
}

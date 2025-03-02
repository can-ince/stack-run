using System.Linq;
using UnityEngine;

namespace Game.Scripts.Helpers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
    
        public static T Instance => _instance ??= FindObjectsOfType<T>().FirstOrDefault();
    }
}

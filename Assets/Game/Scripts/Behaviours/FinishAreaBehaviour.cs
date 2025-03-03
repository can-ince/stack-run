using Game.Scripts.Interfaces;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    public class FinishAreaBehaviour : MonoBehaviour
    {
        [SerializeField] private Renderer finishAreaRenderer;
        [SerializeField] private Collider finishAreaCollider;
        
        public Bounds Bounds => finishAreaRenderer.bounds;
        public Collider Collider => finishAreaCollider;
    }
}

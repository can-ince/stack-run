using Cinemachine;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    public class CinemachineOrbitalCameraBehaviour : MonoBehaviour
    {
        [Tooltip("The Cinemachine Virtual Camera with an Orbital Transposer component.")]
         [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    
        [Tooltip("Speed at which the camera orbits around the character (degrees per second).")]
        public float orbitSpeed = 30f;
    
        // A flag to control when the camera should orbit (e.g. during celebration)
        private bool isOrbiting = false;
    
        // Cached reference to the orbital transposer component
        private CinemachineOrbitalTransposer _orbitalTransposer;
        
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;

        void Start()
        {
            if (_virtualCamera != null)
            {
                _orbitalTransposer = _virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
                if (_orbitalTransposer == null)
                {
                    Debug.LogError("No CinemachineOrbitalTransposer component found on the Virtual Camera.");
                }
            }
            else
            {
                Debug.LogError("Virtual Camera reference is missing.");
            }
            
        }

        void Update()
        {
            if (isOrbiting && _orbitalTransposer != null)
            {
                // Increment and wrap the axis value continuously
                _orbitalTransposer.m_XAxis.Value = (_orbitalTransposer.m_XAxis.Value + orbitSpeed * Time.deltaTime);
            }
        }
    
        /// <summary>
        /// Call this method to start the orbiting behavior.
        /// </summary>
        public void StartOrbiting()
        {
            isOrbiting = true;
        }
    
        /// <summary>
        /// Call this method to stop the orbiting behavior.
        /// </summary>
        public void StopOrbiting()
        {
            isOrbiting = false;
        }
    }
}

using Cinemachine;
using Game.Scripts.Behaviours;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private CinemachineVirtualCamera followVCam;
        [SerializeField] private CinemachineOrbitalCameraBehaviour finishVCam;
        private static Camera _camera;
        private static CinemachineBrain _cinemachineBrain;

        public static Camera MainCamera
        {
            get
            {
                if (!_camera) _camera = Camera.main;
                return _camera;
            }
        }

        public static CinemachineBrain MainCameraBrain
        {
            get
            {
                if (!_cinemachineBrain) _cinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
                return _cinemachineBrain;
            }
        }
        
        public void ActivateFinishVCam(Transform followTarget)
        {
            followVCam.gameObject.SetActive(false);
            finishVCam.gameObject.SetActive(true);
            
            finishVCam.VirtualCamera.Follow = followTarget;
            finishVCam.VirtualCamera.LookAt = followTarget;
            finishVCam.StartOrbiting();
        }
        
        public void ActivateFollowVCam(Transform followTarget)
        {
            followVCam.gameObject.SetActive(true);
            followVCam.Follow = followTarget;
            followVCam.LookAt = followTarget;
            
            finishVCam.gameObject.SetActive(false);
            finishVCam.StopOrbiting();
        }

    }
}

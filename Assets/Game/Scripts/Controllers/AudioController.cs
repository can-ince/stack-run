using Game.Scripts.Helpers;
using Game.Scripts.Interfaces;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class AudioController : MonoBehaviour, IAudioController
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip noteClip;
        [SerializeField] private AudioClip blockClip;
        
        private float _originalPitch;

        void Awake()
        {
            _originalPitch = audioSource.pitch;
        }

        public void PlayNote()
        {
            audioSource.clip = noteClip;
            audioSource.Play();
        }

        public void IncreaseNotePitch()
        {
            audioSource.pitch += 0.05f;
        }

        public void ResetNotePitch()
        {
            audioSource.pitch = _originalPitch;
        }

        public void PlayBlockSound()
        {
            audioSource.clip = blockClip;
            audioSource.Play();
        }
    }
}

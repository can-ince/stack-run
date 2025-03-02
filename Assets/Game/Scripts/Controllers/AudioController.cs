using Game.Scripts.Helpers;
using Game.Scripts.Interfaces;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class AudioController : Singleton<AudioController>, IAudioController
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip noteClip;
        
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
    }
}

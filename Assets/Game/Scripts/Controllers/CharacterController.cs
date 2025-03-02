using System;
using System.Collections;
using Game.Scripts.Interfaces;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    public class CharacterController : MonoBehaviour, ICharacterController
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;
        
        private Vector3 _targetPosition;
        private bool _isFalling,_isDancing,_toBeFallen;
        private Coroutine _movementCoroutine;
        
        private static readonly int DanceAnimID = Animator.StringToHash("Dance");
        
        public event Action OnCharacterFell;

        public void Initialize()
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            
            // Set an initial target (could be the starting platform's center)
            _targetPosition = transform.position;
            
            StackController.StackingFailed+=OnPlatformStopped;
            StackController.StackingSucced += OnPlatformPlaced;
        }

        public void Dispose()
        {
            StackController.StackingFailed-=OnPlatformStopped;
            StackController.StackingSucced -= OnPlatformPlaced;

        }
      
        /// <summary>
        /// Called when a new platform is generated. Moves the character to the platform's center.
        /// </summary>
        public void MoveToPlatform(Vector3 platformCenter)
        {
            // Update target position to match the platform's center (y remains unchanged)
            _targetPosition = new Vector3(platformCenter.x, transform.position.y, platformCenter.z);

            _movementCoroutine ??= StartCoroutine(MoveToTarget());
        }

        /// <summary>
        /// Trigger the dance animation if character reached the final platform.
        /// </summary>
        public void PlayCelebration()
        {
            if(animator != null)
            {
                animator.SetTrigger(DanceAnimID);
                _isDancing = true;
            }
        }
        
        /// <summary>
        /// Trigger the falling behavior if there is no new platform.
        /// </summary>
        public void Fall()
        {
            if (!_isFalling)
            {
                _isFalling = true;
                // Enable gravity so the character falls naturally
                rb.useGravity = true;  
                rb.isKinematic = false;

                // todo: Trigger falling animation here
                
                // Inform the GameManager to handle game over state
                OnCharacterFell?.Invoke();
                
                Debug.Log("Character is falling! Game over should be triggered.");
            }
        }
        
        private void OnPlatformPlaced(IStackPlatform lastPlatform)
        {
            MoveToPlatform(lastPlatform.GameObject.transform.position);
        }

        private void OnPlatformStopped(IStackPlatform lastPlatform)
        {
            _toBeFallen = true;
            var platformPosition = lastPlatform.GameObject.transform.position;

            var targetPos = new Vector3(transform.position.x, platformPosition.y, platformPosition.z); 
            MoveToPlatform(targetPos);
        }
        
        private IEnumerator MoveToTarget()
        {
            while (!_isFalling)
            {
                Vector3 currentPos = transform.position;
                Vector3 desiredPos = new Vector3(_targetPosition.x, currentPos.y, _targetPosition.z);
                var elapsedTime = 0f;
                var duration = Vector3.Distance(currentPos, desiredPos) / moveSpeed;
                if (duration == 0)
                {
                    transform.position = desiredPos;
                    _movementCoroutine = null;

                    yield break;
                }
                while (elapsedTime < duration)
                {
                    transform.position = Vector3.Lerp(currentPos, desiredPos, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    
                    // Check if the character is standing on the ground
                    if (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, 10f,
                            LayerMask.GetMask("Stack")))
                    {
                        Fall();
                    }
                    
                    yield return null;
                }

                transform.position = desiredPos; // Ensure exact positioning
                
            }

            _movementCoroutine = null;
        }

    }
}

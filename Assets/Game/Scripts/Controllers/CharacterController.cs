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
        private bool _isFalling, _isDancing, _finished;
        private Coroutine _movementCoroutine;

        private static readonly int DanceAnimID = Animator.StringToHash("Dance");
        private static readonly int RunAnimID = Animator.StringToHash("Run");

        public event Action OnFellFromPlatform;
        public event Action OnReachedToFinish;

        public void Initialize()
        {
            rb.useGravity = false;
            rb.isKinematic = true;

            // Set an initial target (could be the starting platform's center)
            _targetPosition = transform.position;

            StackController.StackingFailed += OnPlatformStopped;
            StackController.StackingSucced += OnPlatformPlaced;
        }

        public void Dispose()
        {
            _finished = false;

            StackController.StackingFailed -= OnPlatformStopped;
            StackController.StackingSucced -= OnPlatformPlaced;

            StopMoving();
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
            if (animator != null)
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
                OnFellFromPlatform?.Invoke();

                Debug.Log("Character is falling! Game over should be triggered.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Finish"))
            {
                OnReachedToFinishPlatform();
            }
        }

        private void OnReachedToFinishPlatform()
        {
            if (_finished) return;
            _finished = true;
            StopMoving();

            OnReachedToFinish?.Invoke();

            PlayCelebration();
        }

        private void OnPlatformPlaced(IStackPlatform lastPlatform)
        {
            MoveToPlatform(lastPlatform.GameObject.transform.position);

            animator.SetTrigger(RunAnimID);
        }

        private void OnPlatformStopped(IStackPlatform lastPlatform)
        {
            var platformPosition = lastPlatform.GameObject.transform.position;

            var targetPos = new Vector3(transform.position.x, platformPosition.y, platformPosition.z);
            MoveToPlatform(targetPos);
        }

        private IEnumerator MoveToTarget()
        {
            while (!_isFalling)
            {
                var currentPos = transform.position;
                var desiredPos = new Vector3(_targetPosition.x, currentPos.y, _targetPosition.z);
                var moveDir = (desiredPos - currentPos).normalized;

                var distanceZ = _targetPosition.z - transform.position.z;
                var distanceThreshold = 0.2f;
                
                if (distanceZ < distanceThreshold) // Check if passed target position
                {
                    // Move forward
                    transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(moveDir * moveSpeed * Time.deltaTime);
                }

                // Check if the character is standing on the ground
                if (!Physics.BoxCast(transform.position + Vector3.up, new Vector3(0.25f, 0.1f, 0.25f), Vector3.down,
                        out var hit, Quaternion.identity, 10f, LayerMask.GetMask("Ground")))
                {
                    Fall();
                }

                yield return null;
                
            }

            _movementCoroutine = null;
        }

        private void StopMoving()
        {
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
                _movementCoroutine = null;
            }
        }
    }
}
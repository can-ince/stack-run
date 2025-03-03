using System;
using System.Collections;
using Game.Scripts.Interfaces;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    public class StackPlatformBehaviour : MonoBehaviour, IStackPlatform
    {
        public static event Action<IStackPlatform> PlatformDriftedAway;

        [SerializeField] private Renderer platformRenderer;
        [SerializeField] private Collider platformCollider;
        [SerializeField] private Rigidbody platformRigidbody;
        private float _moveSpeed;
        private bool _isMoving;
        private Vector3 _moveDir;
        private Coroutine _ableToStackControlRoutine;
        private IStackPlatform _previousPlatform;
        private float _originalWidth;

        public Bounds Bounds => platformRenderer.bounds;
        public Collider Collider => platformCollider;
        public GameObject GameObject => gameObject;

        public Rigidbody Rigidbody => platformRigidbody;


        private void Awake()
        {
            // Assuming platformRenderer is assigned.
            _originalWidth = platformRenderer.bounds.size.x;
        }

        public void Initialize(IStackPlatform previousPlatform, Vector3 moveDir, float moveSpeed, Material colorMat)
        {
            _previousPlatform = previousPlatform;
            _moveDir = moveDir;
            _moveSpeed = moveSpeed;
            platformRenderer.material = colorMat;
        }

        public void Dispose()
        {
        }

        private void Update()
        {
            if (_isMoving)
            {
                transform.Translate(_moveDir * _moveSpeed * Time.deltaTime);
            }
        }

        public void StartMoving()
        {
            _isMoving = true;

            _ableToStackControlRoutine ??= StartCoroutine(AbleToStackControlRoutine(_previousPlatform));
        }

        public void StopMoving()
        {
            _isMoving = false;

            if (_ableToStackControlRoutine != null)
            {
                StopCoroutine(_ableToStackControlRoutine);
                _ableToStackControlRoutine = null;
            }
        }

        public bool TryCutStackPlatform(IStackPlatform previousPlatform)
        {
            if (!TryGetOverlapInfo(previousPlatform,
                    out var overlapLength,
                    out var cutLength,
                    out var cutFront,
                    out var newCenterX))
            {
                Debug.Log("No overlap: The platform is completely overflowing, GAME OVER!");

                // Set the platform rb kinematic to false and let it fall   
                platformRigidbody.isKinematic = false;

                return false;
            }

            // If there is a piece to be cut, create a falling piece
            if (cutLength > 0f)
            {
                CreateFallingPiece(cutLength, cutFront);
            }

            // Update the platform according to the new size and position
            UpdatePlatform(overlapLength, newCenterX);

            Debug.Log("Platform cut. Remaining length: " + overlapLength + ", Falling piece length: " + cutLength);

            return true;
        }

        /// <summary>
        /// Calculates the overlap information between two platforms.
        /// </summary>
        private bool TryGetOverlapInfo(IStackPlatform previousPlatform,
            out float overlapLength, out float cutLength, out bool cutFront, out float newCenterX)
        {
            // Get previous and current bounds (world space)
            var prevBounds = previousPlatform.Collider.bounds;
            var currBounds = Collider.bounds;

            // Use bounds.min and bounds.max directly (world-space coordinates)
            var prevXMin = prevBounds.min.x;
            var prevXMax = prevBounds.max.x;
            var currXMin = currBounds.min.x;
            var currXMax = currBounds.max.x;

            // Calculate the overlapping region along the X-axis.
            var overlapXMin = Mathf.Max(prevXMin, currXMin);
            var overlapXMax = Mathf.Min(prevXMax, currXMax);
            overlapLength = overlapXMax - overlapXMin;

            if (overlapLength <= 0f)
            {
                cutLength = 0f;
                newCenterX = transform.position.x;
                cutFront = false;
                return false;
            }

            // Total X-length of the current platform
            var currentLength = currXMax - currXMin;
            // The piece to be cut is what’s missing from the current platform
            cutLength = currentLength - overlapLength;

            // Determine the cutting direction based on relative X positions.
            cutFront = transform.position.x > previousPlatform.GameObject.transform.position.x;
            newCenterX = (overlapXMin + overlapXMax) * 0.5f;

            return true;
        }

        /// <summary>
        /// Updates the remaining platform piece according to the new size and position.
        /// </summary>
        private void UpdatePlatform(float overlapLength, float newCenterX)
        {
            // Calculate the new localScale.x such that:
            // new local scale * original unscaled width = overlapLength.
            float newScaleX = overlapLength / _originalWidth;
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(newScaleX, currentScale.y, currentScale.z);

            // Position the platform so that its center aligns with the center of the overlap.
            transform.position = new Vector3(newCenterX, transform.position.y, transform.position.z);
        }

        /// <summary>
        /// Creates the overflowing part (cut piece) and leaves it to the effect of the physics engine.
        /// </summary>
        private void CreateFallingPiece(float cutLength, bool cutFront)
        {
            // Dimensions of the cut-off piece:
            var fallingPieceScale = new Vector3(cutLength / _originalWidth, transform.localScale.y,
                transform.localScale.z);

            // The current platform's X-axis bounds.
            var currXMin = Bounds.min.x;
            var currXMax = Bounds.max.x;

            // The center of the falling part is located on the side where the overflowing part is located.
            float fallingPieceCenterX = cutFront
                ? currXMax - cutLength * .5f
                : currXMin + cutLength * .5f;

            Vector3 fallingPiecePosition = new Vector3(fallingPieceCenterX, transform.position.y, transform.position.z);

            // Create a copy of the existing platform to obtain the object representing the overhanging part.
            var fallingPiece = Instantiate(gameObject, fallingPiecePosition, transform.rotation);

            fallingPiece.transform.localScale = fallingPieceScale;

            // Remove the component so that the behavior does not work on the falling piece.
            Destroy(fallingPiece.GetComponent<StackPlatformBehaviour>());
            //Add a Rigidbody so that it can be interacted with by the physics engine.
            fallingPiece.TryGetComponent<Rigidbody>(out var fallingPieceRb);

            if (fallingPieceRb == null)
                fallingPieceRb = fallingPiece.AddComponent<Rigidbody>();

            fallingPieceRb.isKinematic = false;
            fallingPieceRb.AddForce(cutFront ? Vector3.right : -Vector3.right, ForceMode.VelocityChange);
            // Falling piece destroyed within a certain period of time for automatic cleaning.
            Destroy(fallingPiece, 3f); //todo: can be pooled
        }

        /// <summary>
        /// Checks if the platform is able to stack with the previous platform
        /// If player does not give any input, platform will drift away and game will end
        /// </summary>
        private IEnumerator AbleToStackControlRoutine(IStackPlatform previousPlatform)
        {
            var direction = _moveDir.x;

            // max positionX can go without missing the platform 
            var maxStackPosX = previousPlatform.Collider.bounds.size.x * direction +
                               previousPlatform.GameObject.transform.position.x;

            var inLimit = true;
            // wait until the platform reaches the limit
            while (inLimit)
            {
                if (_moveDir.x > 0)
                {
                    inLimit = maxStackPosX > transform.position.x;
                }
                else
                {
                    inLimit = maxStackPosX < transform.position.x;
                }

                yield return null;
            }

            OnPlatformDriftedAway();
        }

        private void OnPlatformDriftedAway()
        {
            PlatformDriftedAway?.Invoke(this);

            // Set the platform rb kinematic to false and let it fall   
            platformRigidbody.isKinematic = false;

            StopMoving();
        }
    }
}
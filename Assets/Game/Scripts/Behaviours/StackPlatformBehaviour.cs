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
        public Bounds Bounds => platformRenderer.bounds;
        public Collider Collider => platformCollider;
        public GameObject GameObject => gameObject;

        public Rigidbody Rigidbody => platformRigidbody;

        public void Initialize(IStackPlatform previousPlatform,Vector3 moveDir,float moveSpeed, Material colorMat)
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
                // todo: game over   

                // Set the platform rb kinematic to false and let it fall   
                platformRigidbody.isKinematic = false;
                
                return false;
            }
            
            // Update the platform according to the new size and position
            UpdatePlatform(overlapLength, newCenterX);

            // If there is a piece to be cut, create a falling piece
            if (cutLength > 0f)
            {
                CreateFallingPiece(cutLength, cutFront);
            }
            
            Debug.Log("Platform cut. Remaining length: " + overlapLength + ", Falling piece length: " + cutLength);

            return true;
        }

        /// <summary>
        /// Calculates the overlap information between two platforms.
        /// </summary>
        private bool TryGetOverlapInfo(IStackPlatform previousPlatform,
            out float overlapLength, out float cutLength, out bool cutFront, out float newCenterX)
        {
            var prevCollider = previousPlatform.Collider;
            var prevPos = previousPlatform.GameObject.transform.position;
            var prevScale= previousPlatform.GameObject.transform.localScale;
            
            // Calculate the previous platform's X-axis limits
            var prevXMin = prevPos.x - (prevCollider.bounds.size.x * prevScale.x * .5f);
            var prevXMax = prevPos.x + (prevCollider.bounds.size.x * prevScale.x * .5f);
            
            // The current platform's X-axis bounds.
            var currXMin = transform.position.x - (Collider.bounds.size.x * transform.localScale.x * .5f);
            var currXMax = transform.position.x + (Collider.bounds.size.x * transform.localScale.x * .5f);

            // Calculate the overlap area.
            var overlapXMin = Mathf.Max(prevXMin, currXMin);
            var overlapXMax = Mathf.Min(prevXMax, currXMax);
            overlapLength = overlapXMax - overlapXMin;

            // If there is no overlap, the platform is completely overflown → game failed
            if (overlapLength <= 0f)
            {
                cutLength = 0f;
                newCenterX = transform.position.x;
                cutFront = false;
                return false;
            }

            // Total X length of current platform:
            var currentLength = currXMax - currXMin;
            // Length of the part to be cut:
            cutLength = currentLength - overlapLength;

            // Defaults direction of motion to positive X.
            // If the platform is in front of the previous platform, the overhang is at the front (cutFront = true).
            cutFront = (transform.position.x > prevPos.x);
            newCenterX = (overlapXMin + overlapXMax) * .5f;
            
            return true;
        }

        /// <summary>
        /// Updates the remaining platform piece according to the new size and position.
        /// </summary>
        private void UpdatePlatform(float overlapLength, float newCenterX)
        {
            // Set the new size of the remaining part (overlap):
            var newScaleX = overlapLength / Collider.bounds.size.x; // localScale factor
            var currentScale = transform.localScale;
            transform.localScale = new Vector3(newScaleX, currentScale.y, currentScale.z);

            // The centre of the platform should be placed in the middle of the overlap zone:
            transform.position = new Vector3(newCenterX, transform.position.y, transform.position.z);
        }

        /// <summary>
        /// Creates the overflowing part (cut piece) and leaves it to the effect of the physics engine.
        /// </summary>
        private void CreateFallingPiece(float cutLength, bool cutFront)
        {
            // Dimensions of the cut-off piece:
            var fallingPieceScale = new Vector3(cutLength / Collider.bounds.size.x, transform.localScale.y, transform.localScale.z);

            // The current platform's X-axis bounds.
            var currXMin = transform.position.x - (Collider.bounds.size.x * transform.localScale.x * .5f);
            var currXMax = transform.position.x + (Collider.bounds.size.x * transform.localScale.x * .5f);
            
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
                fallingPieceRb=fallingPiece.AddComponent<Rigidbody>();

            fallingPieceRb.isKinematic = false;
            
            // Falling piece destroyed within a certain period of time for automatic cleaning.
            Destroy(fallingPiece, 3f);//todo: can be pooled
        }
        
        /// <summary>
        /// Checks if the platform is able to stack with the previous platform
        /// If player does not give any input, platform will drift away and game will end
        /// </summary>
        private IEnumerator AbleToStackControlRoutine(IStackPlatform previousPlatform)
        {
            var direction = _moveDir.x;
            
            // max positionX can go without missing the platform 
            var maxStackPosX = previousPlatform.Collider.bounds.size.x*direction +
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
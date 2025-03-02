using System.Collections.Generic;
using Game.Scripts.Behaviours;
using Game.Scripts.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Controllers
{
    public class StackController : Singleton<StackController>
    {
        [Header("Platform Settings")] 
        [SerializeField] private StackPlatformBehaviour platformPrefab;
        [SerializeField] private int poolSize = 10;
        [SerializeField] private float platformSpawnDistance = 5f;
        [SerializeField] private float platformMoveSpeed = 3f;
        [SerializeField] private Transform platformParent;
        [SerializeField] private Transform poolParent;
        [SerializeField] private Material[] stackColors;

        private Queue<StackPlatformBehaviour> _platformPool = new Queue<StackPlatformBehaviour>();
        private List<StackPlatformBehaviour> _stacks = new List<StackPlatformBehaviour>();
        public StackPlatformBehaviour CurrentPlatform { get; private set; }

        void Awake()
        {
            InitializePool();
            Initialize();
        }

        public void Initialize()
        {
            SpawnNewPlatform();
            OnGameStarted();
        }

        public void Dispose()
        {
        }

        private void OnGameStarted()
        {
            SpawnNewPlatform();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StopAndProcessCurrentPlatform();
            }
        }

        /// <summary>
        /// Creating stack pool for later use
        /// </summary>
        void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                StackPlatformBehaviour platform = Instantiate(platformPrefab, poolParent);
                platform.gameObject.SetActive(false);
                _platformPool.Enqueue(platform);
            }
        }

        /// <summary>
        /// Getting platform from pool or creating new one
        /// </summary>
        StackPlatformBehaviour GetPlatformFromPool()
        {
            if (_platformPool.Count > 0)
            {
                var platform = _platformPool.Dequeue();
                platform.gameObject.SetActive(true);
                platform.transform.SetParent(platformParent);
                return platform;
            }
            else
            {
                var platform = Instantiate(platformPrefab, platformParent);
                return platform;
            }
        }

        /// <summary>
        /// Recycle platform back to pool
        /// </summary>
        public void ReturnPlatformToPool(StackPlatformBehaviour platform)
        {
            platform.gameObject.SetActive(false);
            platform.transform.SetParent(poolParent);
            _platformPool.Enqueue(platform);
        }

        /// <summary>
        ///  Spawns a new platform at the next location and updates the active platform.
        /// </summary>
        public void SpawnNewPlatform()
        {
            var isInitialPlatform = _stacks.Count == 0;
            var newPlatform = GetPlatformFromPool();

            //  Randomized spawn pos left or right
            var randomDir = Random.Range(0, 2) == 0 ? 1 : -1;
            var targetDir = randomDir == 1 ? Vector3.right : Vector3.left;

            // initial platform spawns at zero
            var newSpawnPosition = !isInitialPlatform
                ? CurrentPlatform.transform.position +
                  Vector3.forward * platformPrefab.Bounds.size.z +
                  targetDir * platformSpawnDistance
                : Vector3.zero;
            
            newPlatform.transform.position = newSpawnPosition;
            newPlatform.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            
            // Set platform scale based on previous platform
            newPlatform.transform.localScale = isInitialPlatform
                ? platformPrefab.transform.localScale
                : CurrentPlatform.transform.localScale;
            
            newPlatform.Initialize(-randomDir * Vector3.right, platformMoveSpeed,
                stackColors[Random.Range(0, stackColors.Length)]);

            if (!isInitialPlatform)
                newPlatform.StartMoving();

            CurrentPlatform = newPlatform;
            _stacks.Add(CurrentPlatform);
        }


        /// <summary>
        /// Stops the active platform, performs cutting control and spawns a new platform.
        /// </summary>
        private void StopAndProcessCurrentPlatform()
        {
            if (CurrentPlatform != null)
            {
                CurrentPlatform.StopMoving();

                if (ShouldCutPlatform(CurrentPlatform))
                {
                    // Cut the platform according to previous platform
                    CurrentPlatform.CutStackPlatform(_stacks[^2]);
                }
                else
                {
                    //todo: play a sound                        
                }

                SpawnNewPlatform();
            }
        }

        /// <summary>
        ///  Determines if the platform should be cut.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns>true if platform should be cut </returns>
        private bool ShouldCutPlatform(StackPlatformBehaviour platform)
        {
            var cutThreshold = 0.1f;
            var diff = Mathf.Abs(platform.transform.position.x - _stacks[^2].transform.position.x);
            return diff > cutThreshold;
        }
        
    }
}
using System;
using System.Collections.Generic;
using Game.Scripts.Behaviours;
using Game.Scripts.Data;
using Game.Scripts.Interfaces;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Game.Scripts.Controllers
{
    public class StackController : MonoBehaviour, IStackController
    {
        public event Action<IStackPlatform> StackingFailed;
        public event Action<IStackPlatform> StackingSucceed; 

        [Header("Platform Settings")] 
        [SerializeField] private int poolSize = 10;
        [SerializeField] private float platformSpawnDistance = 5f;
        [SerializeField] private float platformMoveSpeed = 3f;
        [SerializeField] private float platformCutThreshold = 0.1f; //how much distance to cut the platform 
        [SerializeField] private FinishAreaBehaviour finishPlatformPrefab;
        [SerializeField] private Transform platformParent;
        [SerializeField] private Transform poolParent;
        [SerializeField] private Material[] stackColors;
        
        [Inject] private IStackPlatform _platformPrefab; //Injecting as Prefab Interface 
        [Inject] private DiContainer _container; // Zenject's DI Container
        private IAudioController _audioController;
        private bool _isStackingEnabled;
        private int _perfectStackComboCounter;
        private LevelData _currentLevelData;
        private FinishAreaBehaviour _currentFinishPlatform;
        private Bounds _currentAnchorPlatformBounds;
        private Queue<IStackPlatform> _platformPool = new Queue<IStackPlatform>();
        private List<IStackPlatform> _stacks = new List<IStackPlatform>();
        public IStackPlatform CurrentPlatform { get; private set; }
        public Bounds AnchorPlatformBounds => _currentAnchorPlatformBounds;
        
        // Zenject will inject the dependency.
        [Inject]
        public void Construct(IAudioController audioController)
        {
            _audioController = audioController;
        }
        void Awake()
        {
            InitializePool();
        }

        public void Initialize(LevelData levelData)
        {
            _currentLevelData = levelData;
            
            GameController.GameStarted+=OnGameStarted;
            GameController.GameEnded+=OnGameEnded;
            SetupLevel();
        }

        public void Dispose()
        {
            GameController.GameStarted-=OnGameStarted;
            GameController.GameEnded-=OnGameEnded;

        }

        void Update()
        {
            if(!_isStackingEnabled) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                StopAndProcessCurrentPlatform();
            }
        }
        
        /// <summary>
        /// Sets up the level by positioning finish platform and finish line.
        /// </summary>
        public void SetupLevel()
        {
            //recycle previous platforms if had any
            foreach (var stackPlatform in _stacks)
            {
                stackPlatform.Dispose();
                
                ReturnPlatformToPool(stackPlatform);
            }
            _stacks.Clear();
            
            //initial platform
            SpawnNewPlatform();
            
            // Calculate the total distance from the starting platform
            // (assumes platforms are placed along the Z-axis).
            float totalDistance = _currentLevelData.stackCount * _platformPrefab.Bounds.size.z +
                                  finishPlatformPrefab.Bounds.extents.z - _platformPrefab.Bounds.extents.z;
            Vector3 finishPlatformPosition = _stacks[0].GameObject.transform.position + Vector3.forward * totalDistance;

            // Instantiate the finish platform at the calculated position.
            _currentFinishPlatform = Instantiate(finishPlatformPrefab, finishPlatformPosition, Quaternion.identity);
            
        }

        /// <summary>
        /// Creating stack pool for later use
        /// </summary>
        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var newPlatform = _container.InstantiatePrefabForComponent<IStackPlatform>(_platformPrefab.GameObject, poolParent);

                newPlatform.GameObject.SetActive(false);
                _platformPool.Enqueue(newPlatform);
            }
        }

        /// <summary>
        /// Getting platform from pool or creating new one
        /// </summary>
        private IStackPlatform GetPlatformFromPool()
        {
            if (_platformPool.Count > 0)
            {
                var platform = _platformPool.Dequeue();
                platform.GameObject.gameObject.SetActive(true);
                platform.GameObject.transform.SetParent(platformParent);
                return platform;
            }
            else
            {
                var platform =  _container.InstantiatePrefabForComponent<IStackPlatform>(_platformPrefab.GameObject, platformParent);
                return platform;
            }
        }

        /// <summary>
        /// Recycle platform back to pool
        /// </summary>
        public void ReturnPlatformToPool(IStackPlatform platform)
        {
            platform.GameObject.SetActive(false);
            platform.GameObject.transform.SetParent(poolParent);
            _platformPool.Enqueue(platform);
        }

        /// <summary>
        ///  Spawns a new platform at the next location and updates the active platform.
        /// </summary>
        private IStackPlatform SpawnNewPlatform()
        {
            var isInitialPlatform = _stacks.Count == 0;
            var newPlatform = GetPlatformFromPool();
            
            //  Randomized spawn pos left or right
            var randomDir = Random.Range(0, 2) == 0 ? 1 : -1;
            var targetDir = randomDir == 1 ? Vector3.right : Vector3.left;

            // initial platform spawns at last anchor platform
            var newSpawnPosition = !isInitialPlatform
                ? CurrentPlatform.GameObject.transform.position + Vector3.forward * _platformPrefab.Bounds.size.z + targetDir * platformSpawnDistance
                : _currentAnchorPlatformBounds.center+ (_currentAnchorPlatformBounds.extents.z)*Vector3.forward;
            
            newPlatform.GameObject.transform.position = newSpawnPosition;
            newPlatform.GameObject.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            
            // Set platform scale based on previous platform
            newPlatform.GameObject.transform.localScale = isInitialPlatform
                ? _platformPrefab.GameObject.transform.localScale
                : CurrentPlatform.GameObject.transform.localScale;
            
            newPlatform.Initialize(CurrentPlatform,-randomDir * Vector3.right, platformMoveSpeed,
                stackColors[Random.Range(0, stackColors.Length)]);

            if (!isInitialPlatform)
                newPlatform.StartMoving();

            CurrentPlatform = newPlatform;
            _stacks.Add(CurrentPlatform);

            return newPlatform;
        }
        
        /// <summary>
        /// Stops the active platform, performs cutting control and spawns a new platform.
        /// </summary>
        private void StopAndProcessCurrentPlatform()
        {
            if (CurrentPlatform == null) return;
            
            CurrentPlatform.StopMoving();

            if (CheckForPerfectPlacement(CurrentPlatform))
            {
                // increase the note pitch with every perfect stack combo count
                if (_perfectStackComboCounter > 0)
                    _audioController.IncreaseNotePitch();
                
                // play a sound  
                _audioController.PlayNote();
                
                _perfectStackComboCounter++;

                OnStackingSucceed(CurrentPlatform);

            }
            else
            {
                // Cut the platform according to previous platform
                if (CurrentPlatform.TryCutStackPlatform(_stacks[^2]))
                {
                    OnStackingSucceed(CurrentPlatform);
                }
                else
                {
                    //The platform is completely overflowing, GAME OVER!
                    OnStackingFailed(CurrentPlatform);
                }
                
                _perfectStackComboCounter = 0;
                _audioController.ResetNotePitch();
                _audioController.PlayBlockSound();
            }
        }
        
        private void OnGameStarted()
        {
            _isStackingEnabled = true;
            SpawnNewPlatform();
            
            _audioController.ResetNotePitch();
            _perfectStackComboCounter = 0;

        }
        
        private void OnGameEnded(bool success)
        {
            if (success)
            {
                _currentAnchorPlatformBounds = _currentFinishPlatform.Bounds;
            }
        }
        
        /// <summary>
        /// Player did not give any input for a stacking duration 
        /// </summary>
        private void OnPlatformDriftedAway(IStackPlatform platform)
        {
            OnStackingFailed(platform);
        }

        /// <summary>
        /// Player failed to add another platform to the stack
        /// </summary>
        private void OnStackingFailed(IStackPlatform platform)
        {
            _isStackingEnabled = false;
            StackingFailed?.Invoke(platform);
        }

        /// <summary>
        /// Reached to target stack count for current level
        /// </summary>
        private void OnStackingCompletedForCurrentLevel()
        {
            _isStackingEnabled = false;
        }

        /// <summary>
        /// Player successfully added another platform to the stack
        /// </summary>
        private void OnStackingSucceed(IStackPlatform platform)
        {
            StackingSucceed?.Invoke(CurrentPlatform);

            if (_stacks.Count < _currentLevelData.stackCount)
            {
                SpawnNewPlatform();
            }
            else
            {
                OnStackingCompletedForCurrentLevel();
            }
        }

        /// <summary>
        ///  Determines if the platform should be cut.
        /// </summary>
        /// <returns>false if platform should be cut </returns>
        private bool CheckForPerfectPlacement(IStackPlatform platform)
        {
            var diff = Mathf.Abs(platform.GameObject.transform.position.x - _stacks[^2].GameObject.transform.position.x);
            return diff < platformCutThreshold;
        }
        
    }
}
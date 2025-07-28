using UnityEngine;
using VContainer;
using Common.Interfaces;

namespace ShaveRunner
{
    public class ImprovedLevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject levelChunkPrefab;
        [SerializeField] private float chunkLength = 20f;
        [SerializeField] private int initialChunks = 3;
        [SerializeField] private int maxChunks = 5;
        [SerializeField] private Transform chunkParent;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private float endZ = 100f;

        [Header("Pooling Settings")]
        [SerializeField] private int preWarmCount = 10;

        [Inject] private IObjectPoolService ObjectPoolService { get; set; }
        [Inject] private IEventBus EventBus { get; set; }
        [Inject] private IGameStateManager GameStateManager { get; set; }

        private int _chunksSpawned = 0;
        private float _lastChunkEndZ = 0f;
        private bool _levelCompleted = false;

        void Start()
        {
            InitializeLevel();
            
            // Subscribe to events
            if (EventBus != null)
            {
                EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (EventBus != null)
            {
                EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
            }
        }

        private void InitializeLevel()
        {
            if (ObjectPoolService != null && levelChunkPrefab != null)
            {
                // Pre-warm the object pool
                ObjectPoolService.PreWarm(levelChunkPrefab.GetComponent<Transform>(), preWarmCount, chunkParent);
            }

            // Spawn initial chunks
            for (int i = 0; i < initialChunks; i++)
            {
                SpawnChunk();
            }

            // Set game state to playing
            GameStateManager?.ChangeState(GameState.Playing);
        }

        private void OnPlayerMoved(PlayerMovedEvent playerEvent)
        {
            if (_levelCompleted) return;

            // Check if we need to spawn new chunks
            if (playerEvent.Position.z + chunkLength > _lastChunkEndZ && _chunksSpawned < maxChunks)
            {
                SpawnChunk();
            }

            // Check for level completion
            if (playerEvent.Position.z >= endZ)
            {
                CompleteLevel();
            }

            // Publish level progress event
            float progress = Mathf.Clamp01(playerEvent.Position.z / endZ);
            EventBus?.Publish(new LevelProgressEvent
            {
                Progress = progress,
                Distance = playerEvent.Position.z
            });
        }

        private void SpawnChunk()
        {
            if (ObjectPoolService == null || levelChunkPrefab == null) return;

            Vector3 spawnPos = new Vector3(0, 0, _lastChunkEndZ);
            var chunkTransform = ObjectPoolService.Get(levelChunkPrefab.GetComponent<Transform>(), chunkParent);
            
            chunkTransform.position = spawnPos;
            chunkTransform.rotation = Quaternion.identity;
            
            _lastChunkEndZ += chunkLength;
            _chunksSpawned++;

            Debug.Log($"Spawned chunk {_chunksSpawned} at position {spawnPos}");
        }

        private void CompleteLevel()
        {
            if (_levelCompleted) return;
            
            _levelCompleted = true;
            
            // Publish game over event
            EventBus?.Publish(new GameOverEvent
            {
                FinalScore = CalculateFinalScore(),
                Distance = player.position.z,
                IsWin = true
            });

            // Change game state
            GameStateManager?.ChangeState(GameState.GameOver);

            // Show win screen
            if (winScreen != null)
            {
                winScreen.SetActive(true);
            }

            Debug.Log("Level completed!");
        }

        private float CalculateFinalScore()
        {
            // Simple scoring based on distance traveled
            return player.position.z * 10f;
        }

        public void RestartLevel()
        {
            _levelCompleted = false;
            _chunksSpawned = 0;
            _lastChunkEndZ = 0f;

            // Clear existing chunks
            ObjectPoolService?.ClearPool(levelChunkPrefab.GetComponent<Transform>());

            // Reset player position
            if (player != null)
            {
                player.position = Vector3.zero;
            }

            // Hide win screen
            if (winScreen != null)
            {
                winScreen.SetActive(false);
            }

            // Restart level
            InitializeLevel();
        }

        public float GetLevelProgress()
        {
            return player != null ? Mathf.Clamp01(player.position.z / endZ) : 0f;
        }

        public float GetDistanceTraveled()
        {
            return player != null ? player.position.z : 0f;
        }
    }
}

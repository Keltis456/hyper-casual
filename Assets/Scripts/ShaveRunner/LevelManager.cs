using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Common.Interfaces;
using ILogger = Common.Interfaces.ILogger;

namespace ShaveRunner
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private GameObject levelChunkPrefab;
        [SerializeField] private float chunkLength = 20f;
        [SerializeField] private int initialChunks = 3;
        [SerializeField] private int maxChunks = 5;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private float endZ = 100f;

        [Inject] private PlayerController player { get; set; }
        [Inject] private IObjectPoolService ObjectPoolService { get; set; }
        [Inject] private IEventBus EventBus { get; set; }
        [Inject] private IGameStateManager GameStateManager { get; set; }
        [Inject] private IConfigurationService ConfigService { get; set; }
        [Inject] private ILogger Logger { get; set; }
        [Inject] private GPUGrassRenderer GrassRenderer { get; set; }

        private int _chunksSpawned = 0;
        private float _lastChunkEndZ = 0f;
        private bool _levelCompleted = false;
        private readonly List<Transform> _activeChunks = new();

        void Start()
        {
            if (player == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(player)} not injected!");
            }
            
            if (ObjectPoolService == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(ObjectPoolService)} not injected!");
            }
            
            if (EventBus == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(EventBus)} not injected!");
            }
            else
            {
                EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            }
            
            if (GameStateManager == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(GameStateManager)} not injected!");
            }
            
            if (ConfigService == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(ConfigService)} not injected!");
            }
            
            if (GrassRenderer == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(GrassRenderer)} not injected!");
            }
            
            InitializeLevel();
        }

        void OnDestroy()
        {
            if (EventBus != null)
            {
                EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
            }
        }

        private void InitializeLevel()
        {
            if (ObjectPoolService != null && levelChunkPrefab != null)
            {
                int preWarmCount = ConfigService.ObjectPoolPreWarmCount;
                ObjectPoolService.PreWarm(levelChunkPrefab.GetComponent<Transform>(), preWarmCount, transform);
            }

            for (int i = 0; i < initialChunks; i++)
            {
                SpawnChunk();
            }

            GameStateManager?.ChangeState(GameState.Playing);
        }

        private void OnPlayerMoved(PlayerMovedEvent playerEvent)
        {
            if (_levelCompleted) return;

            if (playerEvent.Position.z + chunkLength > _lastChunkEndZ && _chunksSpawned < maxChunks)
            {
                SpawnChunk();
            }

            DespawnOldChunks(playerEvent.Position.z);

            if (playerEvent.Position.z >= endZ)
            {
                CompleteLevel();
            }

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
            var chunkTransform = ObjectPoolService.Get(levelChunkPrefab.GetComponent<Transform>(), transform);
            
            chunkTransform.position = spawnPos;
            chunkTransform.rotation = Quaternion.identity;
            
            var grassChunk = chunkTransform.GetComponent<GrassChunk>();
            if (grassChunk != null && GrassRenderer != null)
            {
                grassChunk.Initialize(GrassRenderer);
            }
            
            _activeChunks.Add(chunkTransform);
            _lastChunkEndZ += chunkLength;
            _chunksSpawned++;

            Logger?.LogDebug($"Spawned chunk {_chunksSpawned} at position {spawnPos}");
        }

        private void DespawnOldChunks(float playerZ)
        {
            if (ObjectPoolService == null) return;

            for (int i = _activeChunks.Count - 1; i >= 0; i--)
            {
                var chunk = _activeChunks[i];
                if (chunk != null && playerZ - chunk.position.z > chunkLength * 3f)
                {
                    ObjectPoolService.Return(chunk);
                    _activeChunks.RemoveAt(i);
                    Logger?.LogDebug($"Despawned chunk at {chunk.position.z}");
                }
            }
        }

        private void CompleteLevel()
        {
            if (_levelCompleted) return;
            
            _levelCompleted = true;
            
            EventBus?.Publish(new GameOverEvent
            {
                FinalScore = CalculateFinalScore(),
                Distance = player.transform.position.z,
                IsWin = true
            });

            GameStateManager?.ChangeState(GameState.GameOver);

            if (winScreen != null)
            {
                winScreen.SetActive(true);
            }

            Logger?.LogInfo("Level completed!");
        }

        private float CalculateFinalScore()
        {
            return player.transform.position.z * Common.GameConstants.ScoreMultiplier;
        }

        public void RestartLevel()
        {
            _levelCompleted = false;
            _chunksSpawned = 0;
            _lastChunkEndZ = 0f;

            foreach (var chunk in _activeChunks)
            {
                if (chunk != null)
                {
                    ObjectPoolService?.Return(chunk);
                }
            }
            _activeChunks.Clear();

            if (player != null)
            {
                player.transform.position = Vector3.zero;
            }

            if (winScreen != null)
            {
                winScreen.SetActive(false);
            }

            InitializeLevel();
        }

        public float GetLevelProgress()
        {
            return player != null ? Mathf.Clamp01(player.transform.position.z / endZ) : 0f;
        }

        public float GetDistanceTraveled()
        {
            return player != null ? player.transform.position.z : 0f;
        }
    }
}

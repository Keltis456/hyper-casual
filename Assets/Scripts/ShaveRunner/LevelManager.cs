using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Common.Interfaces;
using ILogger = Common.Interfaces.ILogger;

namespace ShaveRunner
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level References")]
        [SerializeField] private GameObject levelChunkPrefab;

        [Inject] private PlayerController Player { get; set; }
        [Inject] private IObjectPoolService ObjectPoolService { get; set; }
        [Inject] private IGameStateManager GameStateManager { get; set; }
        [Inject] private IConfigurationService ConfigService { get; set; }
        [Inject] private ILogger Logger { get; set; }
        [Inject] private GPUGrassRenderer GrassRenderer { get; set; }

        private int _chunksSpawned;
        private float _lastChunkEndZ;
        private bool _levelCompleted = false;
        private float _actualChunkLength;
        private readonly List<Transform> _activeChunks = new();

        void Start()
        {
            if (Player == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(Player)} not injected!");
            }
            else
            {
                Player.OnPlayerMoved += OnPlayerMoved;
            }
            
            if (ObjectPoolService == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: {nameof(ObjectPoolService)} not injected!");
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
            if (Player != null)
            {
                Player.OnPlayerMoved -= OnPlayerMoved;
            }
        }

        private void InitializeLevel()
        {
            if (ConfigService == null)
            {
                Logger?.LogError($"{nameof(LevelManager)}: Cannot initialize level, ConfigService is null!");
                return;
            }

            _actualChunkLength = ConfigService.ChunkLength;

            if (_actualChunkLength <= 0f)
            {
                _actualChunkLength = 20f;
                Logger?.LogError($"{nameof(LevelManager)}: Invalid chunk length, using fallback value of 20f");
            }

            if (ObjectPoolService != null && levelChunkPrefab != null)
            {
                int preWarmCount = ConfigService.ObjectPoolPreWarmCount;
                ObjectPoolService.PreWarm(levelChunkPrefab.GetComponent<Transform>(), preWarmCount, transform);
            }

            for (int i = 0; i < ConfigService.InitialChunks; i++)
            {
                SpawnChunk();
            }

            GameStateManager?.ChangeState(GameState.Playing);
        }

        private void OnPlayerMoved(PlayerMovedEvent playerEvent)
        {
            if (_levelCompleted || ConfigService == null) return;

            int maxActiveChunks = ConfigService.MaxChunks;

            if (playerEvent.Position.z >= _lastChunkEndZ - _actualChunkLength && _activeChunks.Count < maxActiveChunks)
            {
                SpawnChunk();
            }

            DespawnOldChunks(playerEvent.Position.z);
        }

        private void SpawnChunk()
        {
            if (ObjectPoolService == null || levelChunkPrefab == null) return;

            Vector3 spawnPos = new Vector3(0, 0, _lastChunkEndZ);
            var chunkTransform = ObjectPoolService.Get(levelChunkPrefab.GetComponent<Transform>(), transform);
            
            chunkTransform.position = spawnPos;
            chunkTransform.rotation = Quaternion.identity;
            chunkTransform.localScale = Vector3.one;
            
            var grassChunk = chunkTransform.GetComponent<GrassChunk>();
            if (grassChunk != null && GrassRenderer != null)
            {
                grassChunk.Initialize(GrassRenderer, ConfigService.ChunkWidth, _actualChunkLength, ConfigService.GrassDensityPerChunk, ConfigService.GrassYOrigin);
            }
            
            _activeChunks.Add(chunkTransform);
            _lastChunkEndZ += _actualChunkLength;
            _chunksSpawned++;

            Logger?.LogDebug($"Spawned chunk {_chunksSpawned} at position {spawnPos}, length: {_actualChunkLength}");
        }

        private void DespawnOldChunks(float playerZ)
        {
            if (ObjectPoolService == null) return;

            float despawnDistance = _actualChunkLength * ConfigService.ChunkDespawnDistanceMultiplier;

            for (int i = _activeChunks.Count - 1; i >= 0; i--)
            {
                var chunk = _activeChunks[i];
                if (chunk != null && playerZ - chunk.position.z > despawnDistance)
                {
                    ObjectPoolService.Return(chunk);
                    _activeChunks.RemoveAt(i);
                    Logger?.LogDebug($"Despawned chunk at {chunk.position.z}");
                }
            }
        }
    }
}

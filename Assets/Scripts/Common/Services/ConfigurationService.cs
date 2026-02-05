using UnityEngine;
using Common.Interfaces;
using Common.Configuration;

namespace Common.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public GameConfig Config { get; private set; }

        public float PlayerForwardSpeed => Config?.playerForwardSpeed ?? 5f;
        public float PlayerHorizontalSpeed => Config?.playerHorizontalSpeed ?? 10f;
        public float PlayerLaneLimit => Config?.playerLaneLimit ?? 3f;
        public float PlayerMovementSmoothing => Config?.playerMovementSmoothing ?? 5f;
        public float GrassCutRadius => Config?.grassCutRadius ?? 1f;
        public float GrassCutDistance => Config?.grassCutDistance ?? 0.5f;
        public int ObjectPoolPreWarmCount => Config?.objectPoolPreWarmCount ?? 10;
        public float ChunkWidth => Config?.chunkWidth ?? 10f;
        public float ChunkLength => Config?.chunkLength ?? 20f;
        public int GrassDensityPerChunk => Config?.grassDensityPerChunk ?? 500;
        public float GrassYOrigin => Config?.grassYOrigin ?? 1f;
        public int InitialChunks => Config?.initialChunks ?? 3;
        public int MaxChunks => Config?.maxChunks ?? 5;
        public float ChunkDespawnDistanceMultiplier => Config?.chunkDespawnDistanceMultiplier ?? 3f;

        public ConfigurationService()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            Config = Resources.Load<GameConfig>(GameConstants.ConfigResourcePath);
            
            if (Config == null)
            {
                Debug.LogWarning($"GameConfig not found at Resources/{GameConstants.ConfigResourcePath}. Using default values.");
                Config = ScriptableObject.CreateInstance<GameConfig>();
            }
        }
    }
}

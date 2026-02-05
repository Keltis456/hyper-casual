using Common.Configuration;

namespace Common.Interfaces
{
    public interface IConfigurationService
    {
        GameConfig Config { get; }
        
        float PlayerForwardSpeed { get; }
        float PlayerHorizontalSpeed { get; }
        float PlayerLaneLimit { get; }
        float PlayerMovementSmoothing { get; }
        float GrassCutRadius { get; }
        float GrassCutDistance { get; }
        int ObjectPoolPreWarmCount { get; }
        float ChunkWidth { get; }
        float ChunkLength { get; }
        int GrassDensityPerChunk { get; }
        float GrassYOrigin { get; }
        int InitialChunks { get; }
        int MaxChunks { get; }
        float ChunkDespawnDistanceMultiplier { get; }
    }
}

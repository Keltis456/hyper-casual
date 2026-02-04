using UnityEngine;

namespace Common.Configuration
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configuration/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Settings")]
        public float playerForwardSpeed = 5f;
        public float playerHorizontalSpeed = 10f;
        public float playerLaneLimit = 3f;
        public float playerMovementSmoothing = 5f;

        [Header("Level Settings")]
        public float chunkLength = 20f;
        public int initialChunks = 3;
        public int maxChunks = 5;
        public float levelEndDistance = 100f;

        [Header("Grass Cutting Settings")]
        public float grassCutRadius = 1f;
        public float grassCutDistance = 0.5f;
        public int grassDensityPerChunk = 500;

        [Header("Performance Settings")]
        public int objectPoolPreWarmCount = 10;
        public int targetFrameRate = 60;
        public bool enableVSync = false;
        public int maxGrassRenderDistance = 50;

        [Header("Input Settings")]
        public float inputSensitivity = 1f;
        public bool enableHapticFeedback = true;

        [Header("Audio Settings")]
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 0.7f;

        [Header("Debug Settings")]
        public bool enableDebugLogs = false;
        public bool showPerformanceStats = false;
        public bool enableGizmos = false;

        void OnValidate()
        {
            // Clamp values to reasonable ranges
            playerForwardSpeed = Mathf.Max(0.1f, playerForwardSpeed);
            playerHorizontalSpeed = Mathf.Max(0.1f, playerHorizontalSpeed);
            playerLaneLimit = Mathf.Max(0.1f, playerLaneLimit);
            playerMovementSmoothing = Mathf.Max(0.1f, playerMovementSmoothing);
            
            chunkLength = Mathf.Max(1f, chunkLength);
            initialChunks = Mathf.Max(1, initialChunks);
            maxChunks = Mathf.Max(initialChunks, maxChunks);
            levelEndDistance = Mathf.Max(chunkLength, levelEndDistance);
            
            grassCutRadius = Mathf.Max(0.1f, grassCutRadius);
            grassCutDistance = Mathf.Max(0.1f, grassCutDistance);
            grassDensityPerChunk = Mathf.Max(1, grassDensityPerChunk);
            
            objectPoolPreWarmCount = Mathf.Max(0, objectPoolPreWarmCount);
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
            maxGrassRenderDistance = Mathf.Max(10, maxGrassRenderDistance);
            
            inputSensitivity = Mathf.Max(0.1f, inputSensitivity);
            
            masterVolume = Mathf.Clamp01(masterVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
        }
    }
}

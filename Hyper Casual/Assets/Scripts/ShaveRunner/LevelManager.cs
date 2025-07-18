using UnityEngine;

namespace ShaveRunner
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        public Transform player;
        public GameObject[] levelChunks; // Prefabs for level chunks
        public float chunkLength = 20f; // Length of each chunk
        public int initialChunks = 3; // How many chunks to spawn at start
        public int maxChunks = 5; // Max number of chunks in scene
        public Transform chunkParent;
        public GameObject winScreen;
        public float endZ = 100f; // Z position for end of level

        private int chunksSpawned = 0;
        private float lastChunkEndZ = 0f;

        void Start()
        {
            // Spawn initial chunks
            for (int i = 0; i < initialChunks; i++)
            {
                SpawnChunk();
            }
        }

        void Update()
        {
            // Spawn new chunk if player is close to the end of the last chunk
            if (player.position.z + chunkLength > lastChunkEndZ && chunksSpawned < maxChunks)
            {
                SpawnChunk();
            }

            // Detect end of level
            if (player.position.z >= endZ)
            {
                TriggerWin();
            }
        }

        // Spawns a new level chunk ahead of the player
        void SpawnChunk()
        {
            int index = Random.Range(0, levelChunks.Length);
            Vector3 spawnPos = new Vector3(0, 0, lastChunkEndZ);
            GameObject chunk = Instantiate(levelChunks[index], spawnPos, Quaternion.identity, chunkParent);
            lastChunkEndZ += chunkLength;
            chunksSpawned++;
        }

        // Triggers the win screen
        void TriggerWin()
        {
            if (winScreen != null && !winScreen.activeSelf)
            {
                winScreen.SetActive(true);
                Time.timeScale = 0f; // Pause the game
            }
        }
    }
} 
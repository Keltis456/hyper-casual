using UnityEngine;
using System.Collections.Generic;

namespace ShaveRunner
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        public Transform player;
        public GameObject levelChunk; // Prefabs for level chunks
        public float chunkLength = 20f; // Length of each chunk
        public int initialChunks = 3; // How many chunks to spawn at start
        public int maxChunks = 5; // Max number of chunks in scene
        public Transform chunkParent;
        public GameObject winScreen;
        public float endZ = 100f; // Z position for end of level

        private int chunksSpawned = 0;
        private float lastChunkEndZ = 0f;

        // --- POOLING ---
        private Queue<GameObject> chunkPool = new(); // prefab -> pool
        private List<GameObject> activeChunks = new();
        // --- END POOLING ---

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

            // Despawn chunks that are far behind the player
            if (activeChunks.Count > 0)
            {
                GameObject oldest = activeChunks[0];
                if (player.position.z - oldest.transform.position.z > chunkLength * 2)
                {
                    DespawnChunk(oldest);
                    activeChunks.RemoveAt(0);
                }
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
            Vector3 spawnPos = new Vector3(0, 0, lastChunkEndZ);
            GameObject chunk = GetChunkFromPool(levelChunk, spawnPos, Quaternion.identity, chunkParent);
            lastChunkEndZ += chunkLength;
            chunksSpawned++;
            activeChunks.Add(chunk);
        }

        // Get a chunk from the pool or instantiate if pool is empty
        GameObject GetChunkFromPool(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
        {
            GameObject chunk;
            if (chunkPool.Count > 0)
            {
                chunk = chunkPool.Dequeue();
                chunk.transform.SetPositionAndRotation(pos, rot);
                chunk.transform.SetParent(parent);
            }
            else
            {
                chunk = Instantiate(prefab, pos, rot, parent);
            }
            return chunk;
        }

        // Hide and pool a chunk (keep it active, just move it far below the map)
        void DespawnChunk(GameObject chunk)
        {
            chunkPool.Enqueue(chunk);
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
using System.Collections.Generic;
using UnityEngine;

public class GrassChunk : MonoBehaviour
{
    public Vector2 size = new Vector2(10f, 10f);
    public int density = 500;
    public float grassYOrigin = 1;
    private GPUGrassRenderer grassRenderer;

    // --- Persistent cut state ---
    [SerializeField] public string chunkId = "";
    private static Dictionary<string, Dictionary<GrassBlade.BladeKey, float>> cutStateCache = new();
    // --- End persistent cut state ---

    void Awake()
    {
        if (string.IsNullOrEmpty(chunkId))
            chunkId = System.Guid.NewGuid().ToString();
    }

    void OnEnable()
    {
        grassRenderer = FindAnyObjectByType<GPUGrassRenderer>();
        grassRenderer?.RegisterChunk(this);
    }

    void OnDisable()
    {
        // Cache cut state before unregistering
        CacheCutState();
        grassRenderer?.UnregisterChunk(this);
    }

    private void CacheCutState()
    {
        var renderer = FindAnyObjectByType<GPUGrassRenderer>();
        if (renderer == null) return;
        var bladeBuffer = renderer.GetBladeBuffer();
        int bladeCount = renderer.GetBladeCount();
        if (bladeBuffer == null || bladeCount == 0) return;
        try
        {
            GrassBlade[] blades = new GrassBlade[bladeCount];
            bladeBuffer.GetData(blades);
            var dict = new Dictionary<GrassBlade.BladeKey, float>();
            foreach (var b in blades)
            {
                if (b.chunkIndex == GetChunkIndex())
                {
                    var key = GrassBlade.BladeKey.FromBlade(b);
                    dict[key] = b.cut;
                }
            }
            cutStateCache[chunkId] = dict;
        }
        catch { }
    }

    // Helper to get this chunk's index in the renderer
    private uint GetChunkIndex()
    {
        var renderer = FindAnyObjectByType<GPUGrassRenderer>();
        if (renderer == null) return 0;
        return (uint)renderer.GetChunkIndex(this);
    }

    // Called by GPUGrassRenderer to get cached cut state for this chunk
    public Dictionary<GrassBlade.BladeKey, float> GetCachedCutStateAndClear()
    {
        if (cutStateCache.TryGetValue(chunkId, out var dict))
        {
            cutStateCache.Remove(chunkId);
            return dict;
        }
        return null;
    }

    public void SpawnGrass(List<GrassBlade> blades, Transform root, uint chunkIndex, Dictionary<GrassBlade.BladeKey, float> previousCuts = null)
    {
        // Deterministic seed based only on chunkId
        int seed = chunkId.GetHashCode();
        var rand = new System.Random(seed);

        for (int i = 0; i < density; i++)
        {
            float x = (float)(rand.NextDouble() * size.x - size.x / 2f);
            float z = (float)(rand.NextDouble() * size.y - size.y / 2f);
            var rotationRandomisationRange = 0.75f;
            float rotation = (float)(rand.NextDouble() * (Mathf.PI * (2 - rotationRandomisationRange) - Mathf.PI * rotationRandomisationRange) + Mathf.PI * rotationRandomisationRange);
            float seedVal = (float)rand.NextDouble();

            Vector3 local = new Vector3(x, grassYOrigin, z); // now relative to chunk

            float cut = 0f;
            if (previousCuts != null)
            {
                var key = GrassBlade.BladeKey.FromData(chunkIndex, local, seedVal, rotation);
                if (previousCuts.TryGetValue(key, out float prevCut))
                    cut = prevCut;
            }

            blades.Add(new GrassBlade
            {
                position = local,
                seed = seedVal,
                cut = cut,
                chunkIndex = chunkIndex,
                rotation = rotation
            });
        }
    }

    public void ReRegister()
    {
        var renderer = FindAnyObjectByType<GPUGrassRenderer>();
        renderer?.UnregisterChunk(this);
        renderer?.RegisterChunk(this);
    }
}
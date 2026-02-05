using System.Collections.Generic;
using UnityEngine;

public class GrassChunk : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(10f, 10f);
    [SerializeField] private int density = 500;
    [SerializeField] private float grassYOrigin = 1;
    [SerializeField] private string chunkId = "";
    
    private GPUGrassRenderer grassRenderer;
    private static Dictionary<string, Dictionary<GrassBlade.BladeKey, float>> cutStateCache = new();

    public Vector2 Size => size;
    public int Density => density;
    public float GrassYOrigin => grassYOrigin;
    public string ChunkId => chunkId;

    void Awake()
    {
        if (string.IsNullOrEmpty(chunkId))
            chunkId = System.Guid.NewGuid().ToString();
    }

    public void Initialize(GPUGrassRenderer renderer)
    {
        grassRenderer = renderer;
    }

    void OnEnable()
    {
        if (grassRenderer == null)
        {
            grassRenderer = FindFirstObjectByType<GPUGrassRenderer>();
        }
        
        grassRenderer?.RegisterChunk(this);
    }

    void OnDisable()
    {
        if (grassRenderer != null)
        {
            CacheCutState();
            grassRenderer.UnregisterChunk(this);
        }
    }

    private void CacheCutState()
    {
        if (grassRenderer == null) return;
        
        var bladeBuffer = grassRenderer.GetBladeBuffer();
        if (bladeBuffer == null) return;
        
        int bladeCount = grassRenderer.GetBladeCount();
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

    private uint GetChunkIndex()
    {
        if (grassRenderer == null) return 0;
        return (uint)grassRenderer.GetChunkIndex(this);
    }

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
        int seed = chunkId.GetHashCode();
        var rand = new System.Random(seed);

        for (int i = 0; i < density; i++)
        {
            float x = (float)(rand.NextDouble() * size.x - size.x / 2f);
            float z = (float)(rand.NextDouble() * size.y - size.y / 2f);
            var rotationRandomisationRange = 0.75f;
            float rotation = (float)(rand.NextDouble() * (Mathf.PI * (2 - rotationRandomisationRange) - Mathf.PI * rotationRandomisationRange) + Mathf.PI * rotationRandomisationRange);
            float seedVal = (float)rand.NextDouble();

            Vector3 local = new Vector3(x, grassYOrigin, z);

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
        grassRenderer?.UnregisterChunk(this);
        grassRenderer?.RegisterChunk(this);
    }
}

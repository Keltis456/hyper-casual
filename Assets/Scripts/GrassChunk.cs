using System.Collections.Generic;
using UnityEngine;

public class GrassChunk : MonoBehaviour
{
    [SerializeField] private Transform graphics;
    [SerializeField] private string chunkId = "";
    
    private GPUGrassRenderer _grassRenderer;
    private static readonly Dictionary<string, Dictionary<GrassBlade.BladeKey, float>> CutStateCache = new();
    private float _chunkWidth;
    private float _chunkLength;
    private int _density;
    private float _grassYOrigin;

    void Awake()
    {
        if (string.IsNullOrEmpty(chunkId))
            chunkId = System.Guid.NewGuid().ToString();
    }

    public void Initialize(GPUGrassRenderer grassRenderer, float width, float length, int grassDensity, float yOrigin)
    {
        _grassRenderer = grassRenderer;
        _chunkWidth = width;
        _chunkLength = length;
        _density = grassDensity;
        _grassYOrigin = yOrigin;
        graphics.localScale = new Vector3(width, 1, length);
    }

    void OnEnable()
    {
        if (_grassRenderer == null)
        {
            _grassRenderer = FindFirstObjectByType<GPUGrassRenderer>();
        }
        
        _grassRenderer?.RegisterChunk(this);
    }

    void OnDisable()
    {
        if (_grassRenderer != null)
        {
            CacheCutState();
            _grassRenderer.UnregisterChunk(this);
        }
    }

    private void CacheCutState()
    {
        if (_grassRenderer == null) return;
        
        var bladeBuffer = _grassRenderer.GetBladeBuffer();
        if (bladeBuffer == null) return;
        
        int bladeCount = _grassRenderer.GetBladeCount();
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
            CutStateCache[chunkId] = dict;
        }
        catch { }
    }

    private uint GetChunkIndex()
    {
        if (_grassRenderer == null) return 0;
        return (uint)_grassRenderer.GetChunkIndex(this);
    }

    public Dictionary<GrassBlade.BladeKey, float> GetCachedCutStateAndClear()
    {
        if (CutStateCache.TryGetValue(chunkId, out var dict))
        {
            CutStateCache.Remove(chunkId);
            return dict;
        }
        return null;
    }

    public void SpawnGrass(List<GrassBlade> blades, Transform root, uint chunkIndex, Dictionary<GrassBlade.BladeKey, float> previousCuts = null)
    {
        int seed = chunkId.GetHashCode();
        var rand = new System.Random(seed);

        for (int i = 0; i < _density*_chunkWidth*_chunkLength; i++)
        {
            float x = (float)((rand.NextDouble() - 0.5) * _chunkWidth);
            float z = (float)((rand.NextDouble() - 0.5) * _chunkLength);
            var rotationRandomisationRange = 0.75f;
            float rotation = (float)(rand.NextDouble() * (Mathf.PI * (2 - rotationRandomisationRange) - Mathf.PI * rotationRandomisationRange) + Mathf.PI * rotationRandomisationRange);
            float seedVal = (float)rand.NextDouble();

            Vector3 local = new Vector3(x, _grassYOrigin, z);

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
}

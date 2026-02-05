using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using Common;

public class GPUGrassRenderer : MonoBehaviour
{
    [SerializeField] private Mesh grassMesh;
    [SerializeField] private Material grassMaterial;

    private ComputeBuffer matrixBuffer;
    private List<GrassChunk> chunks = new List<GrassChunk>();
    private Dictionary<GrassChunk, uint> chunkToStableIndex = new Dictionary<GrassChunk, uint>();
    private uint nextStableIndex = 0;
    private bool dirty = false;

    private ComputeBuffer bladeBuffer;
    private ComputeBuffer argsBuffer;
    private int bladeCount;
    private Bounds drawBounds;

    private readonly uint[] args = new uint[5];
    public ComputeBuffer GetMatrixBuffer()
    {
        return matrixBuffer;
    }

    public void RegisterChunk(GrassChunk chunk)
    {
        if (!chunks.Contains(chunk))
        {
            chunks.Add(chunk);
            if (!chunkToStableIndex.ContainsKey(chunk))
            {
                chunkToStableIndex[chunk] = nextStableIndex++;
            }
            dirty = true;
        }
    }

    public void UnregisterChunk(GrassChunk chunk)
    {
        if (chunks.Remove(chunk))
        {
            chunkToStableIndex.Remove(chunk);
            dirty = true;
        }
    }

    void OnEnable()
    {
        RebuildBuffers();
    }

    void OnDisable()
    {
        bladeBuffer?.Release();
        argsBuffer?.Release();
        matrixBuffer?.Release();
    }

    void Update()
    {
        if (dirty)
        {
            RebuildBuffers();
            dirty = false;
        }

        if (bladeBuffer == null || grassMesh == null || grassMaterial == null) return;

        UpdateBounds();
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, drawBounds, argsBuffer);
    }
    void UpdateBounds()
    {
        if (chunks == null || chunks.Count == 0)
        {
            drawBounds = new Bounds(transform.position, Vector3.one);
            return;
        }

        var first = chunks[0];
        var bounds = new Bounds(first.transform.position, Vector3.zero);

        foreach (var chunk in chunks)
        {
            bounds.Encapsulate(chunk.transform.position);
        }

        float extraMargin = 20f; // grow to cover blade height + wind sway
        bounds.Expand(extraMargin);

        drawBounds = bounds;
    }
    void RebuildBuffers()
    {
        // --- PRESERVE CUT STATE ---
        Dictionary<GrassBlade.BladeKey, float> previousCuts = null;
        if (bladeBuffer != null && bladeCount > 0)
        {
            try
            {
                GrassBlade[] oldBlades = new GrassBlade[bladeCount];
                bladeBuffer.GetData(oldBlades);
                previousCuts = new Dictionary<GrassBlade.BladeKey, float>(oldBlades.Length);
                foreach (var b in oldBlades)
                {
                    var key = GrassBlade.BladeKey.FromBlade(b);
                    previousCuts[key] = b.cut;
                }
            }
            catch (Exception)
            {
            }
        }
        // --- END PRESERVE CUT STATE ---

        if (chunks == null || chunks.Count == 0)
        {
            return;
        }

        List<GrassBlade> blades = new();
        Matrix4x4[] chunkMatrices = new Matrix4x4[GameConstants.MaxMatrixCount];
        for (int i = 0; i < GameConstants.MaxMatrixCount; i++)
        {
            chunkMatrices[i] = Matrix4x4.identity;
        }

        foreach (var chunk in chunks)
        {
            if (chunk == null) continue;
            if (!chunkToStableIndex.TryGetValue(chunk, out uint stableIndex)) continue;

            chunkMatrices[stableIndex] = chunk.transform.localToWorldMatrix;
            var chunkCuts = chunk.GetCachedCutStateAndClear();
            chunk.SpawnGrass(blades, transform, stableIndex, chunkCuts ?? previousCuts);
        }

        bladeCount = blades.Count;
        if (bladeCount == 0)
        {
            return;
        }

        // Create blade buffer
        bladeBuffer?.Release();
        bladeBuffer = new ComputeBuffer(bladeCount, Marshal.SizeOf(typeof(GrassBlade)));
        bladeBuffer.SetData(blades);

        matrixBuffer?.Release();
        matrixBuffer = new ComputeBuffer(GameConstants.MaxMatrixCount, sizeof(float) * 16);
        matrixBuffer.SetData(chunkMatrices);

        grassMaterial.SetBuffer("_ChunkToWorldBuffer", matrixBuffer);
        grassMaterial.SetMatrixArray("_ChunkToWorld", chunkMatrices);
        grassMaterial.SetBuffer("_BladeBuffer", bladeBuffer);
        grassMaterial.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        // Setup indirect args buffer
        args[0] = (grassMesh != null) ? grassMesh.GetIndexCount(0) : 0;
        args[1] = (uint)bladeCount;
        args[2] = (grassMesh != null) ? grassMesh.GetIndexStart(0) : 0;
        args[3] = (grassMesh != null) ? grassMesh.GetBaseVertex(0) : 0;
        args[4] = 0;

        argsBuffer?.Release();
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    public ComputeBuffer GetBladeBuffer() => bladeBuffer;
    public int GetBladeCount() => bladeCount;

    public int GetChunkIndex(GrassChunk chunk)
    {
        if (chunkToStableIndex.TryGetValue(chunk, out uint stableIndex))
            return (int)stableIndex;
        return -1;
    }
}
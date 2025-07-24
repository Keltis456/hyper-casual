using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class GPUGrassRenderer : MonoBehaviour
{
    private ComputeBuffer matrixBuffer;
    public Mesh grassMesh;
    public Material grassMaterial;
    public float areaSize = 100f;

    private List<GrassChunk> chunks = new List<GrassChunk>();
    private bool dirty = false;

    private ComputeBuffer bladeBuffer;
    private ComputeBuffer argsBuffer;
    private int bladeCount;
    private Bounds drawBounds;

    private readonly uint[] args = new uint[5];

    public void RegisterChunk(GrassChunk chunk)
    {
        if (!chunks.Contains(chunk))
        {
            chunks.Add(chunk);
            dirty = true;
        }
    }

    public void UnregisterChunk(GrassChunk chunk)
    {
        if (chunks.Remove(chunk))
            dirty = true;
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
        List<GrassBlade> blades = new();
        List<Matrix4x4> chunkMatrices = new();

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk == null) continue;

            chunkMatrices.Add(chunk.transform.localToWorldMatrix);
            chunk.SpawnGrass(blades, transform, (uint)i); // pass chunk index
        }

        bladeCount = blades.Count;
        drawBounds = new Bounds(transform.position, new Vector3(areaSize, 50f, areaSize));

        bladeBuffer?.Release();
        bladeBuffer = new ComputeBuffer(bladeCount, Marshal.SizeOf(typeof(GrassBlade)));
        bladeBuffer.SetData(blades);
        
        if (matrixBuffer != null) matrixBuffer.Release();
        matrixBuffer = new ComputeBuffer(chunkMatrices.Count, sizeof(float) * 16);
        matrixBuffer.SetData(chunkMatrices);
        grassMaterial.SetBuffer("_ChunkToWorldBuffer", matrixBuffer);
        
        grassMaterial.SetMatrixArray("_ChunkToWorld", chunkMatrices.Take(64).ToArray());
        grassMaterial.SetBuffer("_BladeBuffer", bladeBuffer);
        grassMaterial.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        args[0] = (grassMesh != null) ? grassMesh.GetIndexCount(0) : 0;
        args[1] = (uint)bladeCount;
        args[2] = (grassMesh != null) ? grassMesh.GetIndexStart(0) : 0;
        args[3] = (grassMesh != null) ? grassMesh.GetBaseVertex(0) : 0;
        args[4] = 0;

        argsBuffer?.Release();
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        Debug.Log($"Chunks: {chunks.Count}, Blades: {bladeCount}, Matrices: {chunkMatrices.Count}");
    }

    public ComputeBuffer GetBladeBuffer() => bladeBuffer;
    public int GetBladeCount() => bladeCount;
}
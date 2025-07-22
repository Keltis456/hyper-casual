using UnityEngine;
using System.Runtime.InteropServices;

public class GPUGrassRenderer : MonoBehaviour
{
    [Header("Grass Settings")]
    public Mesh grassMesh;                  // Use a single-triangle mesh
    public Material grassMaterial;         // Should support GPU instancing
    public int bladeCount = 100_000;
    public float areaSize = 100f;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    // args: index count per instance, instance count, start index, base vertex, start instance
    private readonly uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        InitBuffers();
    }

    void InitBuffers()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        Vector4[] positions = new Vector4[bladeCount];
        for (int i = 0; i < bladeCount; i++)
        {
            float x = Random.Range(-areaSize / 2f, areaSize / 2f);
            float z = Random.Range(-areaSize / 2f, areaSize / 2f);
            float seed = Random.value;

            // Local space position; will be transformed in shader
            positions[i] = new Vector4(x, 0f, z, seed);
        }

        positionBuffer = new ComputeBuffer(bladeCount, Marshal.SizeOf(typeof(Vector4)));
        positionBuffer.SetData(positions);
        grassMaterial.SetBuffer("_PositionBuffer", positionBuffer);
        grassMaterial.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        // Indirect draw args buffer
        args[0] = (grassMesh != null) ? grassMesh.GetIndexCount(0) : 0;
        args[1] = (uint)bladeCount;
        args[2] = (grassMesh != null) ? grassMesh.GetIndexStart(0) : 0;
        args[3] = (grassMesh != null) ? grassMesh.GetBaseVertex(0) : 0;
        args[4] = 0;

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {
        if (grassMesh == null || grassMaterial == null)
            return;

        Bounds bounds = new Bounds(transform.position, new Vector3(areaSize, 50f, areaSize));
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, bounds, argsBuffer);
    }

    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
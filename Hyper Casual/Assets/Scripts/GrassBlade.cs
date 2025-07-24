using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct GrassBlade
{
    public Vector3 position;   // Local to chunk
    public float seed;
    public float cut;
    public uint chunkIndex;    // NEW: index to matrix array
    public float rotation;
    private Vector3 _padding;
}
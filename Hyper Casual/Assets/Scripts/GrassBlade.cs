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

    // --- BladeKey struct for identifying blades ---
    public struct BladeKey : System.IEquatable<BladeKey>
    {
        public uint chunkIndex;
        public int posX, posY, posZ;
        public int seedHash;
        public int rotHash;
        public static BladeKey FromBlade(GrassBlade b)
        {
            return FromData(b.chunkIndex, b.position, b.seed, b.rotation);
        }
        public static BladeKey FromData(uint chunkIndex, Vector3 pos, float seed, float rotation)
        {
            int qx = Mathf.RoundToInt(pos.x * 1000f);
            int qy = Mathf.RoundToInt(pos.y * 1000f);
            int qz = Mathf.RoundToInt(pos.z * 1000f);
            int seedHash = seed.GetHashCode();
            int rotHash = rotation.GetHashCode();
            return new BladeKey { chunkIndex = chunkIndex, posX = qx, posY = qy, posZ = qz, seedHash = seedHash, rotHash = rotHash };
        }
        public bool Equals(BladeKey other)
        {
            return chunkIndex == other.chunkIndex && posX == other.posX && posY == other.posY && posZ == other.posZ && seedHash == other.seedHash && rotHash == other.rotHash;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)chunkIndex;
                hash = hash * 31 + posX;
                hash = hash * 31 + posY;
                hash = hash * 31 + posZ;
                hash = hash * 31 + seedHash;
                hash = hash * 31 + rotHash;
                return hash;
            }
        }
    }
}
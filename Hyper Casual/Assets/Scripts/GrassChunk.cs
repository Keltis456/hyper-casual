using System.Collections.Generic;
using UnityEngine;

public class GrassChunk : MonoBehaviour
{
    public Vector2 size = new Vector2(10f, 10f);
    public int density = 500;
    private GPUGrassRenderer grassRenderer;

    void OnEnable()
    {
        grassRenderer = FindAnyObjectByType<GPUGrassRenderer>();
        grassRenderer?.RegisterChunk(this);
    }

    void OnDisable()
    {
        grassRenderer?.UnregisterChunk(this);
    }

    public void SpawnGrass(List<GrassBlade> blades, Transform root, uint chunkIndex)
    {
        for (int i = 0; i < density; i++)
        {
            float x = Random.Range(-size.x / 2f, size.x / 2f);
            float z = Random.Range(-size.y / 2f, size.y / 2f);
            var rotationRandomisationRange = 0.75f;
            float rotation = Random.Range(Mathf.PI*rotationRandomisationRange, Mathf.PI * (2 - rotationRandomisationRange));
            float seed = Random.value;

            Vector3 local = new Vector3(x, 2, z); // now relative to chunk

            blades.Add(new GrassBlade
            {
                position = local,
                seed = seed,
                cut = 0f,
                chunkIndex = chunkIndex,
                rotation = rotation
            });
        }
    }
}
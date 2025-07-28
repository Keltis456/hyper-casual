using UnityEngine;

public class GrassCutter : MonoBehaviour
{
    public ComputeShader cutShader;
    public GPUGrassRenderer grassRenderer;
    public float cutRadius = 1f;

    private int kernel;

    void Start()
    {
        kernel = cutShader.FindKernel("CutGrass");
    }

    Vector3 lastCutPosition;
    float cutDistance = 0.5f; // how often to send a new cut

    void Update()
    {
        Vector3 playerPos = transform.position;

        // Only cut if player moved enough
        if (Vector3.Distance(playerPos, lastCutPosition) > cutDistance)
        {
            CutAtPosition(playerPos);
            lastCutPosition = playerPos;
        }
    }
    
    public void CutAtPosition(Vector3 worldPos)
    {
        ComputeBuffer bladeBuffer = grassRenderer.GetBladeBuffer();
        ComputeBuffer matrixBuffer = grassRenderer.GetMatrixBuffer();

        if (bladeBuffer == null || matrixBuffer == null || grassRenderer.GetBladeCount() == 0)
        {
            Debug.LogWarning("Grass buffers not ready, skipping cut.");
            return;
        }

        cutShader.SetBuffer(kernel, "blades", bladeBuffer);
        cutShader.SetBuffer(kernel, "_ChunkToWorldBuffer", matrixBuffer);

        cutShader.SetVector("cutCenter", worldPos); // ✅ world space!
        cutShader.SetFloat("cutRadius", cutRadius);
        cutShader.SetInt("count", grassRenderer.GetBladeCount());

        int threadGroups = Mathf.CeilToInt(grassRenderer.GetBladeCount() / 256f);
        cutShader.Dispatch(kernel, threadGroups, 1, 1);
    }
}
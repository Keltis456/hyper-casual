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

    public void CutAtPosition(Vector3 worldPos)
    {
        Vector3 local = grassRenderer.transform.InverseTransformPoint(worldPos);

        ComputeBuffer bladeBuffer = grassRenderer.GetBladeBuffer();

        cutShader.SetBuffer(kernel, "blades", bladeBuffer);
        cutShader.SetVector("cutCenter", local);
        cutShader.SetFloat("cutRadius", cutRadius);
        cutShader.SetInt("count", grassRenderer.GetBladeCount());

        int threadGroups = Mathf.CeilToInt(grassRenderer.GetBladeCount() / 256f);
        cutShader.Dispatch(kernel, threadGroups, 1, 1);
    }
}
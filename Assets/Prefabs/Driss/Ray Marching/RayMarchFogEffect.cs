using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RayMarchFogEffect : MonoBehaviour
{
    public Material rayMarchMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (rayMarchMaterial != null)
            Graphics.Blit(src, dest, rayMarchMaterial);
        else
            Graphics.Blit(src, dest);
    }
}

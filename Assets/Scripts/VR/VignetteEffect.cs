using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class VignetteEffect : MonoBehaviour
{
    [SerializeField] private ContinuousMoveProvider moveProvider;
    [SerializeField] private Material vignetteMaterial;

    [Range(0f, 1f)]
    public float vignetteStrength = 0.5f;
    public float fadeSpeed = 3f;

    private float currentStrength = 0f;

    void Update()
    {
        bool isMoving = moveProvider != null &&
                        moveProvider.enabled &&
                        Input.GetAxis("Horizontal") != 0 ||
                        Input.GetAxis("Vertical") != 0;

        float target = isMoving ? vignetteStrength : 0f;
        currentStrength = Mathf.Lerp(currentStrength, target, Time.deltaTime * fadeSpeed);

        if (vignetteMaterial != null)
            vignetteMaterial.SetFloat("_Strength", currentStrength);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (vignetteMaterial != null)
            Graphics.Blit(src, dest, vignetteMaterial);
        else
            Graphics.Blit(src, dest);
    }
}
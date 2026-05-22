using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class VignetteOverlay : MonoBehaviour
{
    [SerializeField] private ContinuousMoveProvider moveProvider;
    [Range(0f, 1f)] public float maxStrength = 0.8f;
    public float fadeSpeed = 3f;

    private Image vignetteImage;
    private float currentAlpha = 0f;

    void Start()
    {
        // Créer le Canvas
        GameObject canvasGO = new GameObject("VignetteCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Créer l'image
        GameObject imageGO = new GameObject("VignetteImage");
        imageGO.transform.SetParent(canvasGO.transform);
        vignetteImage = imageGO.AddComponent<Image>();

        // Générer la texture vignette
        vignetteImage.sprite = CreateVignetteSprite(256, 256);
        vignetteImage.color = new Color(0, 0, 0, 0);

        // Plein écran
        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    Sprite CreateVignetteSprite(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        Vector2 center = new Vector2(0.5f, 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 uv = new Vector2((float)x / width, (float)y / height);
                float dist = Vector2.Distance(uv, center) * 2f;
                float alpha = Mathf.Clamp01(dist - 0.3f) * 1.5f;
                pixels[y * width + x] = new Color(0, 0, 0, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), center);
    }

    void Update()
    {
        bool isMoving = moveProvider != null && moveProvider.enabled;
        float target = isMoving ? maxStrength : 0f;
        currentAlpha = Mathf.Lerp(currentAlpha, target, Time.deltaTime * fadeSpeed);

        if (vignetteImage != null)
            vignetteImage.color = new Color(0, 0, 0, currentAlpha);
    }
}
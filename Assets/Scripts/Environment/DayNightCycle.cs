using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Durée d'une journée complète en secondes")]
    public float dayDuration = 120f; // 2 minutes par défaut
    [Tooltip("Heure actuelle (0-24)")]
    [Range(0, 24)]
    public float timeOfDay = 12f; // Commencer à midi

    [Header("Sun Settings")]
    public Light sunLight;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Ambient Settings")]
    public Gradient ambientColor;
    public Gradient fogColor;

    [Header("Shadows")]
    public float maxShadowDistanceIdx = 150f;

    private void Start()
    {
        // Auto-configuration si les champs sont vides
        if (sunLight == null)
            sunLight = RenderSettings.sun;
            
        if (sunLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    sunLight = l;
                    break;
                }
            }
        }

        // Configuration par défaut si non assigné
        if (sunColor == null || sunColor.colorKeys.Length < 2) SetupDefaults();
    }

    private void Reset()
    {
        SetupDefaults();
    }

    void SetupDefaults()
    {
        // 1. Configuration des couleurs du soleil
        sunColor = new Gradient();
        GradientColorKey[] sunColors = new GradientColorKey[5];
        GradientAlphaKey[] sunAlphas = new GradientAlphaKey[2];

        // Nuit (Noir) -> Aube (Orange) -> Midi (Blanc/Jaune) -> Crépuscule (Rouge) -> Nuit
        sunColors[0] = new GradientColorKey(Color.black, 0.0f);        // 0h
        sunColors[1] = new GradientColorKey(new Color(1f, 0.5f, 0.0f), 0.25f); // 6h (lever)
        sunColors[2] = new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.5f); // 12h (midi)
        sunColors[3] = new GradientColorKey(new Color(1f, 0.3f, 0.0f), 0.75f); // 18h (coucher)
        sunColors[4] = new GradientColorKey(Color.black, 1.0f);        // 24h

        sunAlphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        sunAlphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        sunColor.SetKeys(sunColors, sunAlphas);

        // 2. Configuration de l'intensité
        sunIntensity = new AnimationCurve();
        sunIntensity.AddKey(0.0f, 0.0f);   // 0h
        sunIntensity.AddKey(0.20f, 0.0f);  // 5h (nuit)
        sunIntensity.AddKey(0.25f, 0.5f);  // 6h (lever)
        sunIntensity.AddKey(0.5f, 1.2f);   // 12h (zénith)
        sunIntensity.AddKey(0.75f, 0.5f);  // 18h (coucher)
        sunIntensity.AddKey(0.80f, 0.0f);  // 19h (nuit)
        sunIntensity.AddKey(1.0f, 0.0f);   // 24h

        // 3. Ambient Color (Ciel)
        ambientColor = new Gradient();
        GradientColorKey[] ambColors = new GradientColorKey[3];
        ambColors[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.0f); // Nuit bleu foncé
        ambColors[1] = new GradientColorKey(new Color(0.6f, 0.6f, 0.7f), 0.5f); // Jour gris-bleu
        ambColors[2] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1.0f); // Nuit

        ambientColor.SetKeys(ambColors, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });

        // 4. Fog Color (similaire à l'ambiant mais plus dense)
        fogColor = new Gradient();
        GradientColorKey[] fogColors = new GradientColorKey[4];
        fogColors[0] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0.0f); // Nuit noire
        fogColors[1] = new GradientColorKey(new Color(0.8f, 0.5f, 0.3f), 0.25f);  // Matin brumeux orange
        fogColors[2] = new GradientColorKey(new Color(0.7f, 0.8f, 0.9f), 0.5f);   // Jour clair
        fogColors[3] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 1.0f); // Nuit

        fogColor.SetKeys(fogColors, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
    }

    private void Update()
    {
        // Avancer le temps
        timeOfDay += (Time.deltaTime / dayDuration) * 24f;
        
        // Boucler le temps (0-24)
        if (timeOfDay >= 24) timeOfDay = 0;

        UpdateSun();
        UpdateLighting();
    }

    private void UpdateSun()
    {
        if (sunLight == null) return;

        // Rotation du soleil : 
        // 0h (minuit) = -90° (bas)
        // 6h (lever) = 0° (horizon)
        // 12h (midi) = 90° (haut)
        // 18h (coucher) = 180° (horizon)
        
        // Mappage de 0-24h vers une rotation
        float alpha = timeOfDay / 24f;
        float sunRotation = Mathf.Lerp(-90, 270, alpha);
        
        sunLight.transform.rotation = Quaternion.Euler(sunRotation, 0, 0);

        // Couleur et intensité
        sunLight.color = sunColor.Evaluate(alpha);
        sunLight.intensity = sunIntensity.Evaluate(alpha);

        // Activer/Désactiver les ombres la nuit pour économiser les perfs ou éviter les artefacts
        if (sunLight.intensity < 0.01f && sunLight.shadows != LightShadows.None)
        {
            sunLight.shadows = LightShadows.None;
        }
        else if (sunLight.intensity > 0.01f && sunLight.shadows == LightShadows.None)
        {
            sunLight.shadows = LightShadows.Soft;
        }
    }

    private void UpdateLighting()
    {
        float alpha = timeOfDay / 24f;
        
        // Lumière ambiante
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor.Evaluate(alpha);

        // Couleur du brouillard (si activé)
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = fogColor.Evaluate(alpha);
        }
    }
}

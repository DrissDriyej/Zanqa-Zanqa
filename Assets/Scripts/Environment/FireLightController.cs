using UnityEngine;

public class FireLightController : MonoBehaviour
{
    [Header("Settings")]
    public float baseIntensity = 5.0f;
    public float maxRange = 10.0f;
    
    [Header("Flicker Effect")]
    public bool enableFlicker = true;
    public float flickerSpeed = 10.0f;
    public float flickerAmount = 0.5f;

    private Light fireLight;
    private float initialBaseIntensity;

    void Start()
    {
        // Essayer de récupérer la lumière sur cet objet ou ses enfants
        fireLight = GetComponent<Light>();
        if (fireLight == null)
            fireLight = GetComponentInChildren<Light>();

        if (fireLight == null)
        {
            // Si pas de lumière, on en crée une !
            GameObject lightObj = new GameObject("FireLightSource");
            lightObj.transform.parent = transform;
            lightObj.transform.localPosition = new Vector3(0, 1.5f, 0); // Un peu en hauteur
            fireLight = lightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.5f, 0.2f); // Orange feu
            fireLight.shadows = LightShadows.Soft;
        }

        // Appliquer les réglages initiaux
        fireLight.range = maxRange;
        initialBaseIntensity = baseIntensity;
        fireLight.intensity = baseIntensity;
    }

    void Update()
    {
        if (fireLight == null) return;

        if (enableFlicker)
        {
            // Effet de scintillement avec Perlin Noise
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, transform.position.x);
            fireLight.intensity = initialBaseIntensity + (noise - 0.5f) * flickerAmount * 2.0f;
            
            // Petit mouvement de la lumière pour simuler la flamme qui bouge
            float moveX = (Mathf.PerlinNoise(Time.time * 2f, 0) - 0.5f) * 0.1f;
            float moveZ = (Mathf.PerlinNoise(0, Time.time * 2f) - 0.5f) * 0.1f;
            // On conserve la position Y locale, on bouge juste un peu X et Z
            // Note: cela suppose que la lumière est un enfant ou l'objet lui-même.
        }
    }
    
    // Pour tout appliquer d'un coup dans l'éditeur (hack pour l'agent)
    [ContextMenu("Boost All Fire Lights in Scene")]
    public void BoostAllLights()
    {
        FireLightController[] all = FindObjectsByType<FireLightController>(FindObjectsSortMode.None);
        foreach(var f in all)
        {
            f.baseIntensity = 40.0f; // Boost !
            f.maxRange = 25.0f;
        }
    }
}

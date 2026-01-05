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

    [Header("Particles (Embers)")]
    public bool enableParticles = true;
    public int particleCount = 20;
    public Color emberVertexColor = new Color(1f, 0.6f, 0.4f, 1f);

    private Light fireLight;
    private ParticleSystem fireParticles;
    private float initialBaseIntensity;

    void Start()
    {
        // 1. Setup Light
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

        // Apply settings
        fireLight.range = maxRange;
        initialBaseIntensity = baseIntensity;
        fireLight.intensity = baseIntensity;

        // 2. Setup Particles (Embers)
        if (enableParticles)
        {
            fireParticles = GetComponent<ParticleSystem>();
            if (fireParticles == null) fireParticles = gameObject.AddComponent<ParticleSystem>();
            
            // Main Module
            var main = fireParticles.main;
            main.startColor = emberVertexColor;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;
            
            // Emission
            var emission = fireParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = particleCount;

            // Shape (Cone qui monte)
            var shape = fireParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.2f;
            shape.rotation = new Vector3(-90, 0, 0); // Upwards

            // Velocity over Lifetime (un peu de vent/bougeotte)
            var vel = fireParticles.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

            // Size over Lifetime (ça rétrécit)
            var sz = fireParticles.sizeOverLifetime;
            sz.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 0.0f);
            sz.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            // Renderer
            var renderer = fireParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            // Utiliser une texture par défaut si possible, ou laisser le carré blanc "style low poly" si null
            Texture2D defaultTex = Resources.GetBuiltinResource<Texture2D>("Default-Particle.psd");
            if (defaultTex != null) renderer.material.mainTexture = defaultTex;
            renderer.material.SetColor("_TintColor", emberVertexColor);
        }
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

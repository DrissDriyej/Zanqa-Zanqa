using UnityEngine;

public class FountainJet : MonoBehaviour
{
    [Header("Jet Settings")]
    public float startSpeed = 5.0f;
    public float startSize = 0.1f;
    public float gravity = 1.0f; // Multiplicateur de gravité
    public Color waterColor = new Color(0.6f, 0.9f, 1.0f, 0.5f);
    
    [Header("Collision")]
    public bool collideWithGround = true;
    public float bounce = 0.1f;
    public float dampening = 0.5f;

    void Start()
    {
        // Créer le système de particules
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();
        
        // 1. Main Module
        var main = ps.main;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startColor = waterColor;
        main.gravityModifier = gravity; // C'est ici que la gravité agit !
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.startLifetime = 2.0f; // Durée de vie
        
        // 2. Emission (BEAUCOUP PLUS DE DÉBIT)
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 800; // C'était 200. Là ça va couler fort !
        
        // 3. Shape (Forme du robinet/jet)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5.0f; // Un peu plus évasé (était 2.0)
        shape.radius = 0.05f;
        shape.rotation = new Vector3(-90, 0, 0); // Vers le haut
        
        // 4. Renderer (Apparence)
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch; // L'eau s'étire quand elle tombe vite
        renderer.cameraVelocityScale = 0.0f;
        renderer.velocityScale = 0.2f;
        renderer.lengthScale = 2.0f;
        
        // Material par défaut (Default-Particle)
        renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        renderer.material.mainTexture = Resources.GetBuiltinResource<Texture2D>("Default-Particle.psd");
        renderer.material.SetColor("_TintColor", waterColor);

        // 5. Collision (Coule par terre)
        if (collideWithGround)
        {
            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.dampen = dampening; // L'eau est freinée quand elle touche le sol
            collision.bounce = bounce;    // Rebondit un peu
            collision.lifetimeLoss = 0.2f; // Meurt un peu après collision
        }
    }
    
    // Pour voir l'objet dans la scène même s'il est invisible
    /*
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f); // Montre la direction
    }
    */
}

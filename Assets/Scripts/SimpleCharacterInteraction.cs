using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Version simplifiée du script d'interaction avec le personnage pour minimiser les erreurs potentielles.
/// </summary>
public class SimpleCharacterInteraction : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    [Tooltip("Distance à laquelle le joueur peut interagir")]
    public float interactionDistance = 3f;
    
    [Tooltip("Touche pour interagir")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("Message d'instruction à afficher")]
    public string instructionMessage = "Appuyez sur E pour récupérer vos courses";
    
    [Header("Références")]
    [Tooltip("Canvas du message d'instruction")]
    public Canvas instructionCanvas;
    
    [Tooltip("Texte du message d'instruction")]
    public Text instructionText;
    
    [Tooltip("Canvas du menu de victoire")]
    public Canvas victoryCanvas;
    
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool isGameWon = false;
    
    private void Start()
    {
        // Trouver le joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Joueur non trouvé! Assurez-vous qu'il a le tag 'Player'");
        }
        
        // Initialiser les messages
        SetupInstructionUI();
        
        // Cacher le menu de victoire
        if (victoryCanvas != null)
        {
            victoryCanvas.gameObject.SetActive(false);
        }
    }
    
    private void SetupInstructionUI()
    {
        // Configurer le texte d'instruction
        if (instructionText != null)
        {
            instructionText.text = instructionMessage;
        }
        
        // Cacher l'instruction au début
        if (instructionCanvas != null)
        {
            instructionCanvas.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (isGameWon) return;
        
        CheckPlayerDistance();

        // Vérifier l'appui sur la touche d'interaction (clavier ou gâchette VR)
        bool vrTrigger = false;
        var rightHandDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        if (rightHandDevices.Count > 0)
        {
            rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out vrTrigger);
        }

        if (isPlayerInRange && (Input.GetKeyDown(interactionKey) || vrTrigger))
        {
            WinGame();
        }
    }
    
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        // Calculer la distance au joueur
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Vérifier si le joueur est à portée d'interaction
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distanceToPlayer <= interactionDistance;
        
        // Afficher/masquer le message d'instruction si le statut a changé
        if (wasInRange != isPlayerInRange && instructionCanvas != null)
        {
            instructionCanvas.gameObject.SetActive(isPlayerInRange);
        }
    }
    
    private void WinGame()
    {
        isGameWon = true;
        
        // Masquer le message d'instruction
        if (instructionCanvas != null)
        {
            instructionCanvas.gameObject.SetActive(false);
        }
        
        // Lancer les confettis
        if (playerTransform != null)
        {
            SpawnConfetti(playerTransform.position + Vector3.up * 5f);
        }

        // Lancer la séquence de fin avec délai
        StartCoroutine(VictoryDelay());
    }

    private System.Collections.IEnumerator VictoryDelay()
    {
        yield return new WaitForSeconds(3.0f);

        // Afficher le menu de victoire
        if (victoryCanvas != null)
        {
            victoryCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("VICTOIRE! Vous avez récupéré vos courses!");
        }
        
        // Débloquer la souris
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SpawnConfetti(Vector3 position)
    {
        GameObject confettiObj = new GameObject("VictoryConfetti");
        confettiObj.transform.position = position;
        
        ParticleSystem ps = confettiObj.AddComponent<ParticleSystem>();
        
        // 1. Main
        var main = ps.main;
        main.startLifetime = 5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow); 
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 2. Emission
        var emission = ps.emission;
        emission.rateOverTime = 0; 
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 100) }); 
        
        // 3. Shape
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(5, 1, 5);
        
        // 4. Color over Lifetime
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 0.33f), new GradientColorKey(Color.green, 0.66f), new GradientColorKey(Color.yellow, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        col.color = grad;

        // 5. Rotation
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.x = new ParticleSystem.MinMaxCurve(0, 360);
        rot.y = new ParticleSystem.MinMaxCurve(0, 360);
        rot.z = new ParticleSystem.MinMaxCurve(0, 360);
        
        // 6. Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        Texture2D defaultTex = Resources.GetBuiltinResource<Texture2D>("Default-Particle.psd");
        if (defaultTex != null) renderer.material.mainTexture = defaultTex;
        
        ps.Play();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dessiner la sphère de détection dans l'éditeur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 
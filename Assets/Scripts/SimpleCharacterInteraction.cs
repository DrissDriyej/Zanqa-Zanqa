using UnityEngine;
using UnityEngine.UI;

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
        
        // Vérifier l'appui sur la touche d'interaction
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
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
    
    private void OnDrawGizmosSelected()
    {
        // Dessiner la sphère de détection dans l'éditeur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 
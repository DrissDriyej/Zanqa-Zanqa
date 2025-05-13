using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Version simplifiée du menu de victoire pour minimiser les erreurs potentielles.
/// </summary>
public class SimpleVictoryMenu : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("Nom de la scène du menu principal")]
    public string mainMenuSceneName = "MainMenu";
    
    [Header("Boutons (facultatifs)")]
    public Button replayButton;
    public Button menuButton;
    public Button quitButton;
    
    [Header("Texte (facultatif)")]
    public Text victoryText;
    
    private void OnEnable()
    {
        // S'assurer que le curseur est visible lors de l'affichage du menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Configurer les boutons s'ils existent
        SetupButtons();
    }
    
    private void Start()
    {
        // Configurer les boutons au démarrage également
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => RestartGame());
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => ReturnToMainMenu());
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => QuitGame());
        }
        
        // Configurer le texte de victoire si disponible
        if (victoryText != null)
        {
            victoryText.text = "Félicitations! Vous avez récupéré vos courses!";
        }
    }
    
    /// <summary>
    /// Redémarre la scène de jeu actuelle.
    /// </summary>
    public void RestartGame()
    {
        // Recharger la scène actuelle
        try
        {
            Scene currentScene = SceneManager.GetActiveScene();
            Debug.Log("Redémarrage de la scène: " + currentScene.name);
            SceneManager.LoadScene(currentScene.name);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors du redémarrage de la scène: " + e.Message);
        }
    }
    
    /// <summary>
    /// Retourne au menu principal.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Charger la scène du menu principal
        try
        {
            Debug.Log("Retour au menu principal: " + mainMenuSceneName);
            SceneManager.LoadScene(mainMenuSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors du chargement du menu principal: " + e.Message);
        }
    }
    
    /// <summary>
    /// Quitte le jeu ou arrête le mode lecture dans l'éditeur.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 
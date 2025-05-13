using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Version simplifiée du gestionnaire de menu principal pour minimiser les erreurs potentielles.
/// </summary>
public class SimpleMainMenu : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("Nom de la scène principale du jeu")]
    public string gameSceneName = "Medina";
    
    [Header("Boutons (facultatifs)")]
    public Button startButton;
    public Button quitButton;
    
    private void Start()
    {
        // S'assurer que le curseur est visible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Configurer les boutons s'ils existent
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => LoadGame());
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => QuitGame());
        }
    }
    
    /// <summary>
    /// Charge la scène du jeu principal.
    /// </summary>
    public void LoadGame()
    {
        // Charger la scène de jeu
        try
        {
            Debug.Log("Chargement de la scène: " + gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors du chargement de la scène: " + e.Message);
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
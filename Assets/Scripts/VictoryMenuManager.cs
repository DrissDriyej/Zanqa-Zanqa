using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryMenuManager : MonoBehaviour
{
    [Header("Éléments UI")]
    [Tooltip("Texte du message de victoire")]
    public TextMeshProUGUI victoryText;
    
    [Tooltip("Bouton pour rejouer")]
    public Button replayButton;
    
    [Tooltip("Bouton pour revenir au menu principal")]
    public Button menuButton;
    
    [Tooltip("Bouton pour quitter le jeu")]
    public Button quitButton;
    
    [Header("Paramètres")]
    [Tooltip("Nom de la scène du menu principal")]
    public string menuSceneName = "MainMenu";
    
    [Tooltip("Message de victoire")]
    public string victoryMessage = "Félicitations! Vous avez récupéré vos courses!";
    
    private void Start()
    {
        // Configurer le message de victoire
        if (victoryText != null)
        {
            victoryText.text = victoryMessage;
        }
        
        // Associer les actions aux boutons
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(ReplayGame);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMenu);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // S'assurer que le curseur est visible et débloqué
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void ReplayGame()
    {
        // Recharger la scène actuelle
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    public void ReturnToMenu()
    {
        // Charger la scène du menu principal
        SceneManager.LoadScene(menuSceneName);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 
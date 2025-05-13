using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Éléments UI")]
    public Button playButton;
    public Button quitButton;
    
    [Header("Paramètres")]
    [Tooltip("Nom de la scène principale du jeu")]
    public string gameSceneName = "GameScene";
    
    private void Start()
    {
        // Associer les actions aux boutons
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // S'assurer que le curseur est visible et débloqué
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Activer le son si désactivé
        AudioListener.pause = false;
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
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
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Instance singleton pour accéder facilement au GameManager
    public static GameManager Instance { get; private set; }
    
    [Header("Paramètres de scène")]
    [Tooltip("Nom de la scène du menu principal")]
    public string menuSceneName = "MainMenu";
    
    [Tooltip("Nom de la scène du jeu principal")]
    public string gameSceneName = "GameScene";
    
    // Paramètres de progression du jeu
    private bool _gameStarted = false;
    private bool _gameWon = false;
    
    private void Awake()
    {
        // Configuration du singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Permet au GameManager de persister entre les scènes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Initialiser les paramètres du jeu
        ResetGameState();
    }
    
    public void StartGame()
    {
        _gameStarted = true;
        _gameWon = false;
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void WinGame()
    {
        _gameWon = true;
    }
    
    public void RestartGame()
    {
        ResetGameState();
        StartGame();
    }
    
    public void ReturnToMainMenu()
    {
        ResetGameState();
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
    
    private void ResetGameState()
    {
        _gameStarted = false;
        _gameWon = false;
    }
    
    // Méthodes pour vérifier l'état du jeu
    public bool IsGameStarted() => _gameStarted;
    public bool IsGameWon() => _gameWon;
} 
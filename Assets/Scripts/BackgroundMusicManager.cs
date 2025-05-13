using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère la musique de fond pour différentes scènes du jeu.
/// Pour l'utiliser, créez un GameObject vide et attachez ce script.
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Joue automatiquement la musique au démarrage")]
    public bool playOnStart = true;
    
    [Tooltip("Ne pas détruire cet objet lors du changement de scène")]
    public bool dontDestroyOnLoad = true;
    
    [Tooltip("Utiliser des transitions en fondu")]
    public bool useFadeTransitions = true;
    
    [Header("Musiques")]
    [Tooltip("Musique pour le menu principal")]
    public AudioClip menuMusic;
    
    [Tooltip("Musique pour le jeu")]
    public AudioClip gameMusic;
    
    [Header("Paramètres")]
    [Tooltip("Volume de la musique (0-1)")]
    [Range(0, 1)]
    public float musicVolume = 0.5f;
    
    [Tooltip("Durée des fondus (secondes)")]
    [Range(0.1f, 5f)]
    public float fadeDuration = 1.5f;
    
    [Tooltip("Nom de la scène du menu principal")]
    public string menuSceneName = "MainMenu";
    
    private AudioSource _audioSource;
    private MusicFader _musicFader;
    private string _currentSceneName;
    private static BackgroundMusicManager _instance;
    
    private void Awake()
    {
        // Configuration du singleton pour ne pas avoir de doublons
        if (_instance == null)
        {
            _instance = this;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            // S'assurer qu'il y a un AudioSource sur cet objet
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configurer l'AudioSource
            _audioSource.loop = true;
            _audioSource.volume = musicVolume;
            _audioSource.playOnAwake = false;
            
            // Ajouter le composant MusicFader si nécessaire
            if (useFadeTransitions)
            {
                _musicFader = GetComponent<MusicFader>();
                if (_musicFader == null)
                {
                    _musicFader = gameObject.AddComponent<MusicFader>();
                }
                _musicFader.fadeDuration = fadeDuration;
            }
        }
        else
        {
            // Détruire tout doublon
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playOnStart)
        {
            // Jouer la musique appropriée au démarrage
            PlayMusicForCurrentScene();
        }
        
        // S'abonner à l'événement de changement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Se désabonner de l'événement de changement de scène
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Changer de musique lorsqu'une nouvelle scène est chargée
        if (_currentSceneName != scene.name)
        {
            _currentSceneName = scene.name;
            PlayMusicForCurrentScene();
        }
    }
    
    /// <summary>
    /// Joue la musique adaptée à la scène actuelle.
    /// </summary>
    public void PlayMusicForCurrentScene()
    {
        if (_audioSource == null) return;
        
        Scene currentScene = SceneManager.GetActiveScene();
        _currentSceneName = currentScene.name;
        
        // Sélectionner la musique en fonction de la scène
        AudioClip musicToPlay = null;
        
        if (_currentSceneName == menuSceneName)
        {
            musicToPlay = menuMusic;
        }
        else
        {
            musicToPlay = gameMusic;
        }
        
        // Jouer la musique sélectionnée
        if (musicToPlay != null)
        {
            if (useFadeTransitions && _musicFader != null)
            {
                // Utiliser une transition en fondu
                _musicFader.CrossFade(musicToPlay);
                Debug.Log("Transition en fondu vers: " + musicToPlay.name);
            }
            else
            {
                // Transition directe
                if (_audioSource.clip != musicToPlay || !_audioSource.isPlaying)
                {
                    _audioSource.clip = musicToPlay;
                    _audioSource.volume = musicVolume;
                    _audioSource.Play();
                    Debug.Log("Lecture de la musique: " + musicToPlay.name);
                }
            }
        }
    }
    
    /// <summary>
    /// Arrête la musique de fond.
    /// </summary>
    public void StopMusic()
    {
        if (_audioSource == null) return;
        
        if (useFadeTransitions && _musicFader != null)
        {
            // Arrêt avec fondu de sortie
            _musicFader.FadeOut();
        }
        else if (_audioSource.isPlaying)
        {
            // Arrêt direct
            _audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Change le volume de la musique.
    /// </summary>
    /// <param name="volume">Nouveau volume (0-1)</param>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        
        if (_audioSource != null)
        {
            _audioSource.volume = musicVolume;
        }
    }
} 
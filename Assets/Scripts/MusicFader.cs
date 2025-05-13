using UnityEngine;
using System.Collections;

/// <summary>
/// Permet de créer des transitions en fondu pour la musique.
/// Ce script est conçu pour être utilisé avec BackgroundMusicManager.
/// </summary>
public class MusicFader : MonoBehaviour
{
    [Tooltip("Durée du fondu en secondes")]
    public float fadeDuration = 1.5f;
    
    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// Effectue un fondu d'entrée de la musique.
    /// </summary>
    public void FadeIn()
    {
        if (_audioSource == null) return;
        
        // Si un fondu est déjà en cours, l'arrêter
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // Démarrer un nouveau fondu d'entrée
        _fadeCoroutine = StartCoroutine(FadeAudioSource(_audioSource, fadeDuration, _audioSource.volume, 0f, true));
    }
    
    /// <summary>
    /// Effectue un fondu de sortie de la musique.
    /// </summary>
    public void FadeOut()
    {
        if (_audioSource == null || !_audioSource.isPlaying) return;
        
        // Si un fondu est déjà en cours, l'arrêter
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // Démarrer un nouveau fondu de sortie
        _fadeCoroutine = StartCoroutine(FadeAudioSource(_audioSource, fadeDuration, _audioSource.volume, 0f, false));
    }
    
    /// <summary>
    /// Change la musique avec un fondu enchaîné.
    /// </summary>
    /// <param name="newClip">Nouvelle musique à jouer</param>
    public void CrossFade(AudioClip newClip)
    {
        if (_audioSource == null || newClip == null) return;
        
        // Si un fondu est déjà en cours, l'arrêter
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // Si la même musique est déjà en cours, ne rien faire
        if (_audioSource.clip == newClip && _audioSource.isPlaying)
        {
            return;
        }
        
        // Enregistrer le volume actuel
        float currentVolume = _audioSource.volume;
        
        // Si une musique est déjà en cours, effectuer un fondu de sortie
        if (_audioSource.isPlaying)
        {
            _fadeCoroutine = StartCoroutine(CrossFadeAudioSource(_audioSource, fadeDuration, currentVolume, newClip));
        }
        else
        {
            // Sinon, simplement démarrer la nouvelle musique avec un fondu d'entrée
            _audioSource.clip = newClip;
            _audioSource.volume = 0f;
            _audioSource.Play();
            _fadeCoroutine = StartCoroutine(FadeAudioSource(_audioSource, fadeDuration, 0f, currentVolume, true));
        }
    }
    
    private IEnumerator FadeAudioSource(AudioSource audioSource, float duration, float startVolume, float targetVolume, bool playOnStart)
    {
        // Initialiser le fondu
        float currentTime = 0f;
        
        if (playOnStart)
        {
            audioSource.volume = startVolume;
            audioSource.Play();
        }
        
        // Effectuer le fondu progressivement
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        
        // S'assurer que le volume final est correct
        audioSource.volume = targetVolume;
        
        // Si c'est un fondu de sortie, arrêter la musique
        if (!playOnStart && targetVolume <= 0f)
        {
            audioSource.Stop();
        }
        
        _fadeCoroutine = null;
    }
    
    private IEnumerator CrossFadeAudioSource(AudioSource audioSource, float duration, float startVolume, AudioClip newClip)
    {
        // Effectuer un fondu de sortie
        float currentTime = 0f;
        
        while (currentTime < duration / 2)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / (duration / 2));
            yield return null;
        }
        
        // Changer la musique
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();
        
        // Effectuer un fondu d'entrée
        currentTime = 0f;
        
        while (currentTime < duration / 2)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, currentTime / (duration / 2));
            yield return null;
        }
        
        // S'assurer que le volume final est correct
        audioSource.volume = startVolume;
        
        _fadeCoroutine = null;
    }
} 
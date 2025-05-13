using UnityEngine;

/// <summary>
/// Ce script sert à contourner l'erreur "MissingReferenceException: The variable m_Targets of GameObjectInspector doesn't exist anymore."
/// Il doit être attaché à un objet dans la scène.
/// </summary>
public class ErrorFix : MonoBehaviour
{
    [Tooltip("Si activé, ce composant ne sera pas détruit lors du chargement d'une nouvelle scène")]
    public bool dontDestroyOnLoad = false;
    
    private void Awake()
    {
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void Start()
    {
        // Force le système à réinitialiser certaines références internes
        Resources.UnloadUnusedAssets();
    }
    
    public void FixEditorReferences()
    {
        // Cette méthode peut être appelée manuellement dans l'éditeur si nécessaire
        Debug.Log("Tentative de correction des références de l'éditeur");
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
} 
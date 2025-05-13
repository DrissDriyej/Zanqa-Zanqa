using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterInteraction : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    [Tooltip("Distance à laquelle le joueur peut interagir")]
    public float interactionDistance = 3f;
    
    [Tooltip("Touche pour interagir")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("Message d'instruction à afficher")]
    public string instructionMessage = "Appuyez sur E pour récupérer vos courses";
    
    [Header("Interface utilisateur")]
    [Tooltip("Canvas pour afficher le menu de victoire")]
    public Canvas victoryCanvas;
    
    [Tooltip("Texte pour le message de victoire")]
    public TextMeshProUGUI victoryText;
    
    [Tooltip("Bouton pour rejouer")]
    public Button replayButton;
    
    [Tooltip("Bouton pour revenir au menu principal")]
    public Button menuButton;
    
    [Tooltip("Bouton pour quitter le jeu")]
    public Button quitButton;
    
    [Header("Paramètres de scène")]
    [Tooltip("Nom de la scène du menu principal")]
    public string menuSceneName = "MainMenu";
    
    private Transform playerTransform;
    private Canvas messageCanvas;
    private TextMeshProUGUI messageText;
    private bool isInRange = false;
    private bool gameWon = false;
    
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
            Debug.LogError("Joueur non trouvé! Assurez-vous qu'il a le tag 'Player'");
        }
        
        // Créer le canvas pour le message d'interaction
        SetupMessageCanvas();
        
        // Configurer le menu de victoire
        SetupVictoryMenu();
    }
    
    private void SetupVictoryMenu()
    {
        // Si le canvas de victoire existe déjà, configurer ses boutons
        if (victoryCanvas != null)
        {
            // Désactiver le canvas au démarrage
            victoryCanvas.enabled = false;
            
            // Configurer le bouton Rejouer
            if (replayButton != null)
            {
                replayButton.onClick.AddListener(ReplayGame);
            }
            
            // Configurer le bouton Menu
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ReturnToMenu);
            }
            
            // Configurer le bouton Quitter
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }
    }
    
    private void SetupMessageCanvas()
    {
        // Créer un canvas pour le texte d'instruction
        GameObject canvasObj = new GameObject("MessageCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, 2, 0); // Position au-dessus du personnage
        
        messageCanvas = canvasObj.AddComponent<Canvas>();
        messageCanvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasRenderer>();
        
        RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(3, 1);
        rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        GameObject textObj = new GameObject("MessageText");
        textObj.transform.SetParent(canvasObj.transform);
        
        // Utiliser TMP si disponible, sinon utiliser Text standard
        try
        {
            messageText = textObj.AddComponent<TextMeshProUGUI>();
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = 24;
            messageText.color = Color.white;
        }
        catch
        {
            Debug.LogWarning("TextMeshPro non disponible, utilisation de Text standard");
            Text regularText = textObj.AddComponent<Text>();
            regularText.alignment = TextAnchor.MiddleCenter;
            regularText.fontSize = 24;
            regularText.color = Color.white;
            regularText.text = instructionMessage;
        }
        
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.localPosition = Vector3.zero;
        textRectTransform.sizeDelta = new Vector2(300, 100);
        textRectTransform.localScale = Vector3.one;
        
        // Désactiver le canvas par défaut
        messageCanvas.enabled = false;
        
        if (messageText != null)
        {
            messageText.text = instructionMessage;
        }
    }
    
    private void Update()
    {
        if (gameWon) return; // Ne rien faire si le jeu est déjà gagné
        
        if (playerTransform != null)
        {
            // Vérifier la distance avec le joueur
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isInRange;
            isInRange = distance <= interactionDistance;
            
            // Afficher/cacher le message
            if (isInRange != wasInRange)
            {
                if (messageCanvas != null)
                {
                    messageCanvas.enabled = isInRange;
                }
            }
            
            // Vérifier l'appui sur la touche d'interaction
            if (isInRange && Input.GetKeyDown(interactionKey))
            {
                WinGame();
            }
        }
    }
    
    private void WinGame()
    {
        // Marquer le jeu comme gagné
        gameWon = true;
        
        // Désactiver le message d'interaction
        if (messageCanvas != null)
        {
            messageCanvas.enabled = false;
        }
        
        // Afficher le menu de victoire
        if (victoryCanvas != null)
        {
            victoryCanvas.enabled = true;
            
            if (victoryText != null)
            {
                victoryText.text = "Félicitations! Vous avez récupéré vos courses!";
            }
            
            // S'assurer que le curseur est visible et débloqué
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Si pas de canvas de victoire, créer un menu de victoire simple
            CreateSimpleVictoryMenu();
        }
        
        // Optionnel: jouer un son de victoire si vous en avez un
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
    
    private void CreateSimpleVictoryMenu()
    {
        // Créer un menu de victoire simple
        GameObject victoryObj = new GameObject("VictoryMenu");
        Canvas canvas = victoryObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = victoryObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        victoryObj.AddComponent<GraphicRaycaster>();
        
        // Créer un panneau de fond
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRectTransform = panelImage.GetComponent<RectTransform>();
        panelRectTransform.anchorMin = new Vector2(0, 0);
        panelRectTransform.anchorMax = new Vector2(1, 1);
        panelRectTransform.sizeDelta = Vector2.zero;
        
        // Créer un texte de victoire
        GameObject titleObj = new GameObject("VictoryText");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = null;
        
        try
        {
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "FÉLICITATIONS!\nVous avez récupéré vos courses!";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.green;
        }
        catch
        {
            Text regularText = titleObj.AddComponent<Text>();
            regularText.text = "FÉLICITATIONS!\nVous avez récupéré vos courses!";
            regularText.fontSize = 48;
            regularText.alignment = TextAnchor.MiddleCenter;
            regularText.color = Color.green;
        }
        
        RectTransform titleRectTransform = titleObj.GetComponent<RectTransform>();
        titleRectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titleRectTransform.anchorMax = new Vector2(0.5f, 0.9f);
        titleRectTransform.sizeDelta = new Vector2(800, 200);
        titleRectTransform.anchoredPosition = Vector2.zero;
        
        // Créer un bouton Rejouer
        CreateButton(panelObj, "ReplayButton", "Rejouer", new Vector2(0.5f, 0.5f), new Vector2(300, 60))
            .onClick.AddListener(ReplayGame);
        
        // Créer un bouton Menu Principal
        CreateButton(panelObj, "MenuButton", "Menu Principal", new Vector2(0.5f, 0.35f), new Vector2(300, 60))
            .onClick.AddListener(ReturnToMenu);
        
        // Créer un bouton Quitter
        CreateButton(panelObj, "QuitButton", "Quitter", new Vector2(0.5f, 0.2f), new Vector2(300, 60))
            .onClick.AddListener(QuitGame);
            
        // S'assurer que le curseur est visible et débloqué
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private Button CreateButton(GameObject parent, string name, string text, Vector2 anchorPosition, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        
        // Ajouter un composant Image pour le fond du bouton
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Ajouter un composant Button
        Button button = buttonObj.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 1);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1);
        button.colors = colors;
        
        // Configurer la position et la taille du bouton
        RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
        buttonRectTransform.anchorMin = anchorPosition;
        buttonRectTransform.anchorMax = anchorPosition;
        buttonRectTransform.sizeDelta = size;
        buttonRectTransform.anchoredPosition = Vector2.zero;
        
        // Ajouter du texte au bouton
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // Essayer d'utiliser TextMeshProUGUI, sinon utiliser Text standard
        try
        {
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }
        catch
        {
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
        }
        
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.sizeDelta = Vector2.zero;
        
        return button;
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
    
    private void OnDrawGizmosSelected()
    {
        // Dessiner la sphère de détection dans l'éditeur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
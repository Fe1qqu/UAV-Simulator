using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles main menu interactions, including starting the level creation wizard and exiting the application.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main menu panel GameObject that contains all main menu UI elements.")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Level creation wizard component.")]
    [SerializeField] private LevelCreationWizard levelCreationWizard;

    [Header("Buttons")]
    [Tooltip("Button to start creating a new level.")]
    [SerializeField] private Button createLevelButton;

    [Tooltip("Button to exit the game.")]
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (mainMenuPanel == null)
        {
            Debug.LogError("[MainMenuController] MainMenuPanel is not assigned.");
        }

        if (levelCreationWizard == null)
        {
            Debug.LogError("[MainMenuController] LevelCreationWizard is not assigned.");
        }

        if (createLevelButton == null)
        {
            Debug.LogError("[MainMenuController] CreateLevelButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[MainMenuController] ExitButton is not assigned.");
        }
    }

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        levelCreationWizard.gameObject.SetActive(false);

        createLevelButton.onClick.AddListener(OnCreateLevelClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    //private void OnDestroy()
    //{
    //    createLevelButton.onClick.RemoveListener(OnCreateLevelClicked);
    //    exitButton.onClick.RemoveListener(OnExitClicked);
    //}

    /// <summary>
    /// Called when the Create Level button is clicked. Shows the wizard panel.
    /// </summary>
    private void OnCreateLevelClicked()
    {
        mainMenuPanel.SetActive(false);
        levelCreationWizard.StartWizard();
    }

    /// <summary>
    /// Called when the Exit button is clicked. Exits the application or stops play mode in editor.
    /// </summary>
    private void OnExitClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

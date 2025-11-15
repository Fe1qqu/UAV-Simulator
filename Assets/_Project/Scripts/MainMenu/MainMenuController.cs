using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles main menu interactions, including starting the level creation wizard and exiting the application.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Main menu panel GameObject that contains all main menu UI elements.")]
    public GameObject mainMenuPanel;

    [Tooltip("Level creation wizard component.")]
    public LevelCreationWizard levelCreationWizard;

    [Header("Buttons")]
    [Tooltip("Button to start creating a new level.")]
    public Button createLevelButton;

    [Tooltip("Button to exit the game.")]
    public Button exitButton;

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
        levelCreationWizard.gameObject.SetActive(true);
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

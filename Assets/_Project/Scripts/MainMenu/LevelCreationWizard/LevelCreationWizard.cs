using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the level creation wizard UI, navigation between steps, and starting the level editor.
/// </summary>
public class LevelCreationWizard : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel containing the wizard UI.")]
    public GameObject wizardPanel;

    [Tooltip("Panel for the main menu.")]
    public GameObject mainMenuPanel;

    [Header("Steps")]
    [Tooltip("Array of LevelCreationStep components representing each step of the wizard.")]
    public LevelCreationStep[] steps;

    private int currentStep = 0;

    [Header("Buttons")]
    [Tooltip("Button used to navigate to the next step.")]
    public Button nextButton;

    [Tooltip("Button used to navigate to the previous step.")]
    public Button backButton;

    [Tooltip("Button to return to the main menu.")]
    public Button mainMenuButton;

    private void Start()
    {
        wizardPanel.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    //private void OnDestroy()
    //{
    //    nextButton.onClick.RemoveListener(OnNextClicked);
    //    backButton.onClick.RemoveListener(OnBackClicked);
    //    mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    //}

    /// <summary>
    /// Starts the wizard and shows the first step.
    /// </summary>
    public void StartWizard()
    {
        wizardPanel.SetActive(true);
        ShowStep(0);
    }

    /// <summary>
    /// Shows the step at the given index and hides all others.
    /// Updates the next button text accordingly.
    /// </summary>
    /// <param name="index">Index of the step to show.</param>
    private void ShowStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].gameObject.SetActive(i == index);
        }

        currentStep = index;

        steps[currentStep].OnStepShown();

        nextButton.GetComponentInChildren<TMP_Text>().text = currentStep < steps.Length - 1 ? "Далее" : "Начать";
    }

    /// <summary>
    /// Handles the next button click. Validates current step and moves forward or creates the level.
    /// </summary>
    private void OnNextClicked()
    {
        var step = steps[currentStep];

        if (!step.ValidateStep())
        {
            return;
        }

        if (currentStep == steps.Length - 1)
        {
            CreateLevel();
            return;
        }

        ShowStep(currentStep + 1);
    }

    /// <summary>
    /// Handles the back button click. Moves to the previous step or returns to main menu if at first step.
    /// </summary>
    private void OnBackClicked()
    {
        if (currentStep == 0)
        {
            OnMainMenuClicked();
            return;
        }

        ShowStep(currentStep - 1);
    }

    /// <summary>
    /// Returns to the main menu panel.
    /// </summary>
    private void OnMainMenuClicked()
    {
        wizardPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Loads the LevelEditor scene to create the level.
    /// </summary>
    private void CreateLevel()
    {
        //Debug.Log("[LevelCreationWizard] Creating level and loading LevelEditor scene");
        SceneManager.LoadScene("LevelEditor");
    }
}

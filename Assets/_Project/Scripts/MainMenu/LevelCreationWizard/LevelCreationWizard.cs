using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Components;

/// <summary>
/// Manages the level creation wizard UI, navigation between steps, and starting the level editor.
/// </summary>
public class LevelCreationWizard : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel containing the wizard UI.")]
    [SerializeField] private GameObject wizardPanel;

    [Tooltip("Panel for the main menu.")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Steps")]
    [Tooltip("Array of LevelCreationStep components representing each step of the wizard.")]
    [SerializeField] private LevelCreationStep[] steps;

    [Header("Buttons")]
    [Tooltip("Button used to navigate to the next step.")]
    [SerializeField] private Button nextButton;

    [Tooltip("Button used to navigate to the previous step.")]
    [SerializeField] private Button backButton;

    [Tooltip("Button to return to the main menu.")]
    [SerializeField] private Button mainMenuButton;

    private int currentStep = 0;

    private LocalizeStringEvent nextButtonLocalization;

    private LocationLocalizationPreloader localizationPreloader;

    private void Awake()
    {
        if (wizardPanel == null)
        {
            Debug.LogError("[LevelCreationWizard] WizardPanel is not assigned.");
        }

        if (mainMenuPanel == null)
        {
            Debug.LogError("[LevelCreationWizard] MainMenuPanel is not assigned.");
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("[LevelCreationWizard] Steps array is missing or empty.");
        }

        if (nextButton == null)
        {
            Debug.LogError("[LevelCreationWizard] NextButton is not assigned.");
        }

        if (backButton == null)
        {
            Debug.LogError("[LevelCreationWizard] BackButton is not assigned.");
        }

        if (mainMenuButton == null)
        {
            Debug.LogError("[LevelCreationWizard] MainMenuButton is not assigned.");
        }

        nextButtonLocalization = nextButton.GetComponentInChildren<LocalizeStringEvent>();
        if (nextButtonLocalization == null)
        {
            Debug.LogError("[LevelCreationWizard] LocalizeStringEvent not found on nextButton.");
        }

        localizationPreloader = GetComponent<LocationLocalizationPreloader>();
        if (localizationPreloader == null)
        {
            Debug.LogError("[LevelCreationWizard] LocationLocalizationPreloader not found on this GameObject.");
        }
    }

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
    //    mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    //}

    /// <summary>
    /// Starts the wizard and shows the first step.
    /// </summary>
    public async void StartWizard()
    {
        wizardPanel.SetActive(true);
        ShowStep(0);

        await localizationPreloader.Load();
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

        string key = (currentStep < steps.Length - 1) ? "next" : "start";
        nextButtonLocalization.StringReference.TableEntryReference = key;
        nextButtonLocalization.RefreshString();

        steps[currentStep].OnStepShown();
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
        localizationPreloader.Unload();

        wizardPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Loads the LevelEditor scene to create the level.
    /// </summary>
    private void CreateLevel()
    {
        localizationPreloader.Unload();

        SceneManager.LoadScene("LevelEditor");
    }
}

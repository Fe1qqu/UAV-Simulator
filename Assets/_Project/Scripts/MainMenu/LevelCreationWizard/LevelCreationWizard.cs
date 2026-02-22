using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Components;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Manages the level creation wizard UI, navigation between steps, and starting the level editor.
/// </summary>
public class LevelCreationWizard : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel containing the wizard UI.")]
    [SerializeField] private GameObject wizardPanel;

    [Header("Steps")]
    [Tooltip("Array of LevelCreationWizardStepBase components representing each step of the wizard.")]
    [SerializeField] private LevelCreationWizardStepBase[] steps;

    [Header("Buttons")]
    [Tooltip("Button used to navigate to the next step.")]
    [SerializeField] private Button nextButton;

    [Tooltip("Button used to navigate to the previous step.")]
    [SerializeField] private Button backButton;

    [Tooltip("Button to return to the main menu.")]
    [SerializeField] private Button mainMenuButton;

    [Header("Context")]
    [SerializeField] private MainMenuContext mainMenuContext;

    private int currentStep = 0;

    private LocalizeStringEvent nextButtonLocalization;

    private LocalizationPreloader localizationPreloader;

    private CancellationTokenSource cancellationTokenSource;

    public Action OnExit;
    public Action OnExitToMainMenu;

    private void Awake()
    {
        if (wizardPanel == null)
        {
            Debug.LogError("[LevelCreationWizard] WizardPanel is not assigned.");
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

        if (mainMenuContext == null)
        {
            Debug.LogError("[LevelCreationWizard] MainMenuContext is not assigned.");
        }

        nextButtonLocalization = nextButton.GetComponentInChildren<LocalizeStringEvent>();
        if (nextButtonLocalization == null)
        {
            Debug.LogError("[LevelCreationWizard] LocalizeStringEvent not found on nextButton.");
        }

        localizationPreloader = GetComponent<LocalizationPreloader>();
        if (localizationPreloader == null)
        {
            Debug.LogError("[LevelCreationWizard] LocalizationPreloader not found on this GameObject.");
        }
    }

    private void Start()
    {
        foreach (LevelCreationWizardStepBase step in steps)
        {
            step.Initialize(mainMenuContext);
        }

        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(OnNextClicked);
        backButton.onClick.RemoveListener(OnBackClicked);
        mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    public async Task StartWizardAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();

        await localizationPreloader.Load(cancellationTokenSource.Token);

        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        ShowStep(0);
        wizardPanel.SetActive(true);
    }

    public void StopWizard()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        wizardPanel.SetActive(false);
        localizationPreloader.Unload();
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
            OnExit?.Invoke();
            return;
        }

        ShowStep(currentStep - 1);
    }

    public bool GoBackOneStep()
    {
        if (currentStep == 0)
        {
            return false;
        }

        ShowStep(currentStep - 1);
        return true;
    }

    /// <summary>
    /// Returns to the main menu panel.
    /// </summary>
    private void OnMainMenuClicked()
    {
        localizationPreloader.Unload();
        OnExitToMainMenu?.Invoke();
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

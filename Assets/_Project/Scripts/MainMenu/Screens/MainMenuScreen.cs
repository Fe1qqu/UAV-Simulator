using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
{
    [SerializeField] private Button createLevelButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (createLevelButton == null)
        {
            Debug.LogError("[MainMenuScreen] CreateLevelButton is not assigned.");
        }

        if (settingsButton == null)
        {
            Debug.LogError("[MainMenuScreen] SettingsButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[MainMenuScreen] ExitButton is not assigned.");
        }
    }

    public override void OnShow()
    {
        createLevelButton.onClick.AddListener(OnCreateLevel);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
    }

    public override void OnHide()
    {
        createLevelButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }

    private void OnCreateLevel()
    {
        stateMachine.Show<LevelCreationWizardScreen>();
    }

    private void OnSettings()
    {
        stateMachine.Show<SettingsScreen>();
    }

    private void OnExit()
    {
        stateMachine.ExitGame();
    }
}

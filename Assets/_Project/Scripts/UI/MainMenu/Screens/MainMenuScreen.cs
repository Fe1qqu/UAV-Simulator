using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : MainMenuScreenBase
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelEditorButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (playButton == null)
        {
            Debug.LogError("[MainMenuScreen] PlayButton is not assigned.");
        }

        if (levelEditorButton == null)
        {
            Debug.LogError("[MainMenuScreen] LevelEditorButton is not assigned.");
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
        playButton.onClick.AddListener(OnPlayClicked);
        levelEditorButton.onClick.AddListener(OnCreateLevelClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    public override void OnHide()
    {
        playButton.onClick.RemoveAllListeners();
        levelEditorButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }

    private void OnPlayClicked()
    {
        stateMachine.Show<PlayMenuScreen>();
    }

    private void OnCreateLevelClicked()
    {
        stateMachine.Show<LevelEditorMenuScreen>();
    }

    private void OnSettingsClicked()
    {
        stateMachine.Show<SettingsScreen>();
    }

    private void OnExitClicked()
    {
        stateMachine.ExitGame();
    }
}

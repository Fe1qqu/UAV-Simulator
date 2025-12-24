using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
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
        playButton.onClick.AddListener(OnPlay);
        levelEditorButton.onClick.AddListener(OnCreateLevel);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
    }

    public override void OnHide()
    {
        playButton.onClick.RemoveAllListeners();
        levelEditorButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }

    private void OnPlay()
    {
        stateMachine.Show<PlayMenuScreen>();
    }

    private void OnCreateLevel()
    {
        stateMachine.Show<LevelEditorMenuScreen>();
        //stateMachine.Show<LevelCreationWizardScreen>();
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

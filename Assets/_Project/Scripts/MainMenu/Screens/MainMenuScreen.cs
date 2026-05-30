using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : MainMenuScreenBase
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelEditorButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Modal dialog")]
    [SerializeField] private ModalDialogService modalDialogService;

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

        if (modalDialogService == null)
        {
            Debug.LogError("[MainMenuScreen] ModalDialogService is not assigned.");
        }
    }

    protected override void OnShowInternal()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        levelEditorButton.onClick.AddListener(OnCreateLevelClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    protected override void OnHideInternal()
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
        ShowExitConfirmation();
    }

    public override bool OnBack()
    {
        ShowExitConfirmation();
        return true;
    }

    private void ShowExitConfirmation()
    {
        modalDialogService.Show(new ModalDialogConfig
        {
            messageLocalizationKey = "modal_exit_message",

            Buttons = new[]
            {
                new ModalButtonConfig
                {
                    localizationTableEntryKey = "confirm",
                    Result = ModalResult.Confirm
                },
                new ModalButtonConfig
                {
                    localizationTableEntryKey = "cancel",
                    Result = ModalResult.Cancel,
                    IsBackAction = true
                }
            },

            OnResult = result =>
            {
                if (result == ModalResult.Confirm)
                {
                    stateMachine.ExitGame();
                }
            }
        });
    }
}

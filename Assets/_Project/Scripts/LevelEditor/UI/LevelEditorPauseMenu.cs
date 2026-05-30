using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorPauseMenu : PauseMenuBase
{
    [SerializeField] private LevelEditorManager levelEditorManager;
    [SerializeField] private LevelEditorLevelDataBuilder levelDataBuilder;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

    [Header("Modal dialog")]
    [SerializeField] private ModalDialogService modalDialogService;

    protected override void Awake()
    {
        base.Awake();

        if (levelEditorManager == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] LevelEditorManager is not assigned.");
        }

        if (levelDataBuilder == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] LevelDataBuilder is not assigned.");
        }

        if (continueButton == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] ContinueButton is not assigned.");
        }

        if (saveButton == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] SaveButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] ExitButton is not assigned.");
        }

        if (modalDialogService == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] ModalDialogService is not assigned.");
        }
    }

    private void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveListener(OnContinueClicked);
        saveButton.onClick.RemoveListener(OnSaveClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    public void OnContinueClicked()
    {
        Close();
    }

    public void OnSaveClicked()
    {
        ScenarioValidationResult result = ScenarioValidator.Validate(levelEditorManager.CurrentScenario, LevelObjectRegistry.Instance);
        if (!result.IsValid)
        {
            ShowValidationDialog(result);
            return;
        }

        Debug.Log("[LevelEditorPauseMenu] Saving level...");

        LevelEditorSession levelEditorSession = GameSession.Instance.LevelEditor;
        if (string.IsNullOrEmpty(levelEditorSession.LevelFilePath))
        {
            Debug.Log("[LevelEditorPauseMenu] No level file path. Creating new one.");
            CreateNewLevelFilePath(levelEditorSession);
        }

        LevelData levelData = levelDataBuilder.CollectLevelData();
        levelEditorManager.LevelFileManager.SaveByPath(levelEditorSession.LevelFilePath, levelData);

        ShowSaveSuccessDialog();
    }

    private void ShowValidationDialog(ScenarioValidationResult result)
    {
        modalDialogService.Show(new ModalDialogConfig
        {
            titleLocalizationKey = "validation_save_blocked",

            messageLines = ValidationMessageBuilder.Build(result),

            messageAlignment = TextAlignmentOptions.TopLeft,

            Buttons = new[]
            {
                new ModalButtonConfig
                {
                    localizationTableEntryKey = "ok",
                    Result = ModalResult.Confirm,
                    IsBackAction = true
                }
            }
        });
    }

    private void ShowSaveSuccessDialog()
    {
        modalDialogService.Show(new ModalDialogConfig
        {
            titleLocalizationKey = "validation_save_success",

            Buttons = new[]
            {
                new ModalButtonConfig
                {
                    localizationTableEntryKey = "to_main_menu",
                    Result = ModalResult.Cancel
                },
                new ModalButtonConfig
                {
                    localizationTableEntryKey = "close",
                    Result = ModalResult.Confirm,
                    IsBackAction = true
                }
            },

            OnResult = result =>
            {
                if (result == ModalResult.Cancel)
                {
                    OnExitClicked();
                }
            }
        });
    }

    private void CreateNewLevelFilePath(LevelEditorSession levelEditorSession)
    {
        string fileName = levelEditorSession.LevelName;

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("[LevelEditorPauseMenu] LevelName is empty. Cannot save.");
            return;
        }

        string directory = Path.Combine(Application.persistentDataPath, "levels");
        string path = Path.Combine(directory, fileName + ".json");

        levelEditorSession.SetLevelFilePath(path);
    }

    public async void OnExitClicked()
    {
        Close(PauseCloseMode.SceneExit);

        if (levelEditorManager != null)
        {
            levelEditorManager.UnloadLocalization();
        }

        await SceneFlow.ToMainMenu(true);
    }
}

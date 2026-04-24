using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LevelEditorPauseMenu : PauseMenuBase
{
    [SerializeField] private LevelEditorManager levelEditorManager;
    [SerializeField] private LevelEditorLevelDataBuilder levelDataBuilder;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

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
            Debug.LogError($"[LevelEditorPauseMenu] {result.ErrorType}: {result.Message}.");
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

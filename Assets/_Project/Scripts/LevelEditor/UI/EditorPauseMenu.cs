using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EditorPauseMenu : PauseMenuBase
{
    [SerializeField] private EditorManager editorManager;
    [SerializeField] private EditorLevelDataBuilder levelDataBuilder;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

    protected override void Awake()
    {
        base.Awake();

        if (editorManager == null)
        {
            Debug.LogError("[EditorPauseMenu] EditorManager is not assigned.");
        }

        if (levelDataBuilder == null)
        {
            Debug.LogError("[EditorPauseMenu] LevelDataBuilder is not assigned.");
        }

        if (continueButton == null)
        {
            Debug.LogError("[EditorPauseMenu] ContinueButton is not assigned.");
        }

        if (saveButton == null)
        {
            Debug.LogError("[EditorPauseMenu] SaveButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[EditorPauseMenu] ExitButton is not assigned.");
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
        ScenarioValidationResult result = ScenarioValidator.Validate(editorManager.CurrentScenario, LevelObjectRegistry.Instance);
        if (!result.IsValid)
        {
            Debug.LogError($"[EditorPauseMenu] {result.ErrorType}: {result.Message}.");
            return;
        }

        Debug.Log("[EditorPauseMenu] Saving level...");

        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;
        if (string.IsNullOrEmpty(editorSession.SelectedLevelFilePath))
        {
            Debug.Log("[EditorPauseMenu] No level file path. Creating new one.");
            CreateNewLevelFilePath(editorSession);
        }

        LevelData levelData = levelDataBuilder.CollectLevelData();
        editorManager.LevelFileManager.SaveByPath(editorSession.SelectedLevelFilePath, levelData);
    }

    private void CreateNewLevelFilePath(EditorSession editorSession)
    {
        string fileName = editorSession.LevelName;

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("[EditorPauseMenu] LevelName is empty. Cannot save.");
            return;
        }

        string directory = Path.Combine(Application.persistentDataPath, "levels");
        string path = Path.Combine(directory, fileName + ".json");

        editorSession.SelectedLevelFilePath = path;
    }

    public void OnExitClicked()
    {
        Close(PauseCloseMode.SceneExit);

        if (editorManager != null)
        {
            editorManager.UnloadLocalization();
        }

        SceneLoader.LoadMainMenu();
    }
}

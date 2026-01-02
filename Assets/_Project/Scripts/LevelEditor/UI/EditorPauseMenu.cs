using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class EditorPauseMenu : BasePauseMenu
{
    [SerializeField] private LevelSaveManager levelSaveManager;
    [SerializeField] private EditorLevelDataBuilder levelDataBuilder;
    //[SerializeField] private EditorManager editorManager;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

    protected override void Awake()
    {
        base.Awake();

        if (levelDataBuilder == null)
        {
            Debug.LogError("[EditorPauseMenu] LevelDataBuilder is not assigned.");
        }

        if (levelSaveManager == null)
        {
            Debug.LogError("[EditorPauseMenu] LevelSaveManager is not assigned.");
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

        //if (editorManager == null)
        //{
        //    Debug.LogError("[EditorPauseMenu] EditorManager is not assigned.");
        //}
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
        Debug.Log("[EditorPauseMenu] Saving level...");

        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;
        if (string.IsNullOrEmpty(editorSession.SelectedLevelFilePath))
        {
            Debug.Log("[EditorPauseMenu] No level file path. Creating new one.");
            CreateNewLevelFilePath(editorSession);
        }

        LevelData data = levelDataBuilder.CollectLevelData();
        levelSaveManager.SaveByPath(editorSession.SelectedLevelFilePath, data);
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
        PauseManager.SetPaused(false);

        //if (editorManager != null)
        //{
        //    editorManager.UnloadLocalization();
        //}

        SceneManager.LoadScene("MainMenu");
    }
}
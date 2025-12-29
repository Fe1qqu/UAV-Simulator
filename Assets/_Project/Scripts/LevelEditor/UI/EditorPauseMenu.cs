using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class EditorPauseMenu : MonoBehaviour, IBackHandler
{
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private LevelSaveManager levelSaveManager;
    [SerializeField] private EditorLevelDataBuilder levelDataBuilder;
    //[SerializeField] private EditorManager editorManager;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

    private bool isOpen;

    private void Awake()
    {
        if (pauseMenuRoot == null)
        {
            Debug.LogError("[EditorPauseMenu] PauseMenuRoot is not assigned.");
        }

        pauseMenuRoot.SetActive(false);

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
        Time.timeScale = 1f;

        //if (editorManager != null)
        //{
        //    editorManager.UnloadLocalization(); // если сделаешь такой метод
        //}

        SceneManager.LoadScene("MainMenu");
    }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;

        EditorCameraInput.Instance.enabled = false;
        BackDispatcher.Instance.Register(this);
    }

    public void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;

        EditorCameraInput.Instance.enabled = true;
        BackDispatcher.Instance.Unregister(this);
    }

    public bool OnBack()
    {
        if (!isOpen)
        {
            return false;
        }

        Debug.Log("[EditorPauseMenu] OnBack.");

        Close();
        return true;
    }
}
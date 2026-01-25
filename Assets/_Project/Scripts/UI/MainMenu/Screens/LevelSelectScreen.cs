using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public enum LevelSelectMode
{
    Editor,
    Play
}

public class LevelSelectContext
{
    public LevelSelectMode mode;
    public System.Action onBack;

    public LevelSelectContext(LevelSelectMode mode, System.Action onBack)
    {
        this.mode = mode;
        this.onBack = onBack;
    }
}

public class LevelSelectScreen : MainMenuScreenBase
{
    [Header("UI")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Prefabs")]
    [SerializeField] private LevelSelectItem levelSelectItemPrefab;

    private LevelSelectMode mode;
    private System.Action onBackAction;
    private LevelSelectItem selectedItem;
    private string selectedFile;

    private void Awake()
    {
        if (listRoot == null)
        {
            Debug.LogError("[LevelSelectScreen] ListRoot is not assigned.");
        }

        if (confirmButton == null)
        {
            Debug.LogError("[LevelSelectScreen] ConfirmButton is not assigned.");
        }

        if (backButton == null)
        {
            Debug.LogError("[LevelSelectScreen] BackButton is not assigned.");
        }

        if (levelSelectItemPrefab == null)
        {
            Debug.LogError("[LevelSelectScreen] LevelSelectItemPrefab is not assigned.");
        }
    }

    protected override void OnShowInternal(object context)
    {
        if (context is not LevelSelectContext ctx)
        {
            Debug.LogError("[LevelSelectScreen] Missing context.");
            return;
        }

        mode = ctx.mode;
        onBackAction = ctx.onBack;

        selectedItem = null;
        selectedFile = null;

        confirmButton.interactable = false;

        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(OnBackClicked);

        BuildList();
    }

    protected override void OnHideInternal()
    {
        confirmButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        selectedItem = null;
        selectedFile = null;

        confirmButton.interactable = false;

        onBackAction = null;
    }

    private void BuildList()
    {
        foreach (Transform child in listRoot)
        {
            Destroy(child.gameObject);
        }

        string directory = Path.Combine(Application.persistentDataPath, "levels");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        foreach (string file in Directory.GetFiles(directory, "*.json"))
        {
            LevelSelectItem levelSelectItem = Instantiate(levelSelectItemPrefab, listRoot);
            levelSelectItem.gameObject.SetActive(true);
            levelSelectItem.Setup(file, OnItemSelected);
        }
    }

    private void OnItemSelected(LevelSelectItem item)
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }

        selectedItem = item;
        selectedItem.SetSelected(true);

        selectedFile = item.GetFilePath();
        confirmButton.interactable = true;
    }

    private void OnConfirmClicked()
    {
        if (mode == LevelSelectMode.Editor)
        {
            GameSettings.Instance.CurrentEditorSession.SelectedLevelFilePath = selectedFile;

            SceneManager.LoadScene("LevelEditor");
        }
        else
        {
            PlaySession playSession = GameSettings.Instance.CurrentPlaySession;
            playSession.Clear();
            playSession.LevelFilePath = selectedFile;

            SceneManager.LoadScene("Play");
        }
    }

    private void OnBackClicked()
    {
        onBackAction?.Invoke();
    }

    public override bool OnBack()
    {
        OnBackClicked();
        return true;
    }
}

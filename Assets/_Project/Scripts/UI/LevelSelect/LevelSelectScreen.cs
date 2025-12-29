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

public class LevelSelectScreen : UIScreen, IBackHandler
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

    public override void OnShow(object context)
    {
        var ctx = context as LevelSelectContext;
        if (ctx == null)
        {
            Debug.LogError("[LevelSelectScreen] Missing context.");
            return;
        }

        mode = ctx.mode;
        onBackAction = ctx.onBack;

        selectedItem = null;
        selectedFile = null;

        confirmButton.interactable = false;

        confirmButton.onClick.AddListener(OnConfirm);
        backButton.onClick.AddListener(OnBackClicked);

        BuildList();
    }

    public override void OnHide()
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
            LevelSelectItem item = Instantiate(levelSelectItemPrefab, listRoot);
            item.Setup(file, OnItemSelected);
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

    private void OnConfirm()
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

    public bool OnBack()
    {
        OnBackClicked();
        return true;
    }
}

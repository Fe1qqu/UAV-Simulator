using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public enum LevelSelectMode
{
    Editor,
    Play
}

public class LevelSelectContext
{
    public LevelSelectMode mode;
    public Action onBack;

    public LevelSelectContext(LevelSelectMode mode, Action onBack)
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

    [Header("Context")]
    [SerializeField] private MainMenuContext mainMenuContext;

    private LevelSelectMode mode;
    private Action onBackAction;

    private LevelSelectItem selectedItem;

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

        if (mainMenuContext == null)
        {
            Debug.LogError("[LevelSelectScreen] MainMenuContext is not assigned.");
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
        confirmButton.interactable = false;
        onBackAction = null;
    }

    private void BuildList()
    {
        foreach (Transform child in listRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (LevelCatalogEntry levelCatalogEntry in mainMenuContext.LevelCatalog.GetAll())
        {
            LevelSelectItem levelSelectItemInstance = Instantiate(levelSelectItemPrefab, listRoot);
            levelSelectItemInstance.gameObject.SetActive(true);
            levelSelectItemInstance.Setup(levelCatalogEntry, OnItemSelected);
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

        confirmButton.interactable = true;
    }

    private void OnConfirmClicked()
    {
        if (selectedItem == null)
        {
            Debug.LogError("[LevelSelectScreen] SelectedItem is null.");
            return;
        }

        string filePath = selectedItem.Entry.FilePath;

        if (mode == LevelSelectMode.Editor)
        {
            GameSettings.Instance.CurrentEditorSession.SelectedLevelFilePath = filePath;
            SceneManager.LoadScene("LevelEditor");
        }
        else
        {
            PlaySession playSession = GameSettings.Instance.CurrentPlaySession;
            playSession.Clear();
            playSession.LevelFilePath = filePath;
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

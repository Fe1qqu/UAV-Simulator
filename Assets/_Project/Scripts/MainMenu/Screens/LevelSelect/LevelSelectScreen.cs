using UnityEngine;
using UnityEngine.UI;
using System;

public enum LevelSelectMode
{
    LevelEditor,
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
    [SerializeField] private UIButtonVisual confirmButtonVisual;
    [SerializeField] private Button backButton;

    [Header("Prefabs")]
    [SerializeField] private LevelSelectItem levelSelectItemPrefab;

    [Header("Context")]
    [SerializeField] private MainMenuContext mainMenuContext;

    [Header("Modal dialog")]
    [SerializeField] private ModalDialogService modalDialogService;

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

        if (confirmButtonVisual == null)
        {
            Debug.LogError("[LevelSelectScreen] ConfirmButtonVisual is not assigned.");
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

        if (modalDialogService == null)
        {
            Debug.LogError("[LevelEditorPauseMenu] ModalDialogService is not assigned.");
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
        confirmButtonVisual.SetInteractable(false);

        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(OnBackClicked);

        BuildList();
    }

    protected override void OnHideInternal()
    {
        confirmButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        selectedItem = null;
        confirmButtonVisual.SetInteractable(false);
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
            levelSelectItemInstance.Setup(levelCatalogEntry, OnItemSelected, OnItemDeleted);
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

        confirmButtonVisual.SetInteractable(true);
    }

    private void OnItemDeleted(LevelSelectItem item)
    {
        if (item == null)
        {
            return;
        }

        modalDialogService.Show(new ModalDialogConfig
        {
            messageLocalizationKey = "delete_level_message",

            messageArguments = new object[]
            {
                item.Entry.LevelData.levelName
            },

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
                if (result != ModalResult.Confirm)
                {
                    return;
                }

                DeleteLevel(item);
            }
        });
    }

    private void DeleteLevel(LevelSelectItem item)
    {
        string filePath = item.Entry.FilePath;

        mainMenuContext.LevelFileManager.DeleteByPath(filePath);

        if (selectedItem == item)
        {
            selectedItem = null;
            confirmButtonVisual.SetInteractable(false);
        }

        Destroy(item.gameObject);
    }

    private async void OnConfirmClicked()
    {
        if (selectedItem == null)
        {
            Debug.LogError("[LevelSelectScreen] SelectedItem is null.");
            return;
        }

        string filePath = selectedItem.Entry.FilePath;

        if (mode == LevelSelectMode.LevelEditor)
        {
            GameSession.Instance.LevelEditor.SetLevelFilePath(filePath);
            await SceneFlow.ToLevelEditor();
        }
        else
        {
            GameSession.Instance.Play.SetLevelFilePath(filePath);
            await SceneFlow.ToPlay();
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

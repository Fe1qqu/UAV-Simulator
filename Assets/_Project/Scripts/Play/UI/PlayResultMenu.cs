using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class PlayResultMenu : MonoBehaviour, IBackHandler
{
    [SerializeField] private GameObject root;
    [SerializeField] private PlayManager playManager;

    [Header("Title")]
    [SerializeField] private LocalizeStringEvent titleLocalizeStringEvent;
    [SerializeField] private LocalizedString completedTitle;
    [SerializeField] private LocalizedString failedTitle;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    public bool IsOpen => root.activeSelf;

    private void Awake()
    {
        if (root == null)
        {
            Debug.LogError("[PlayResultMenu] Root is not assigned.");
        }

        if (playManager == null)
        {
            Debug.LogError("[PlayResultMenu] PlayManager is not assigned.");
        }

        if (titleLocalizeStringEvent == null)
        {
            Debug.LogError("[PlayResultMenu] TitleLocalizeStringEvent is not assigned.");
        }

        if (restartButton == null)
        {
            Debug.LogError("[PlayResultMenu] RestartButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[PlayResultMenu] ExitButton is not assigned.");
        }
    }

    private void Start()
    {
        restartButton.onClick.AddListener(OnRestartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }
    public void OpenCompleted()
    {
        Open(completedTitle);
    }

    public void OpenFailed()
    {
        Open(failedTitle);
    }

    public void Open(LocalizedString title)
    {
        if (IsOpen)
        {
            return;
        }

        titleLocalizeStringEvent.StringReference = title;
        titleLocalizeStringEvent.RefreshString();

        root.SetActive(true);
        BackDispatcher.RegisterHandler(this);
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        BackDispatcher.UnregisterHandler(this);
        root.SetActive(false);
    }

    private void OnRestartClicked()
    {
        Close();
        playManager.RestartLevel();
    }

    private async void OnExitClicked()
    {
        Close();
        await SceneFlow.ToMainMenu(true);
    }

    public bool OnBack()
    {
        return true;
    }
}

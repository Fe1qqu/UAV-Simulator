using UnityEngine;
using UnityEngine.UI;

public class PlayPauseMenu : PauseMenuBase
{
    [SerializeField] private PlayManager playManager;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    protected override void Awake()
    {
        base.Awake();

        if (playManager == null)
        {
            Debug.LogError("[PlayPauseMenu] PlayManager is not assigned.");
        }

        if (continueButton == null)
        {
            Debug.LogError("[PlayPauseMenu] ContinueButton is not assigned.");
        }

        if (restartButton == null)
        {
            Debug.LogError("[PlayPauseMenu] RestartButton is not assigned.");
        }

        if (exitButton == null)
        {
            Debug.LogError("[PlayPauseMenu] ExitButton is not assigned.");
        }
    }

    private void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveListener(OnContinueClicked);
        restartButton.onClick.RemoveListener(OnRestartClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    public void OnContinueClicked()
    {
        Close();
    }

    public void OnRestartClicked()
    {
        Close();
        playManager.RestartLevel();
    }

    public async void OnExitClicked()
    {
        Close(PauseCloseMode.SceneExit);
        await SceneFlow.ToMainMenu(true);
    }
}

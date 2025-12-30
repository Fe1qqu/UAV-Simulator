using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayPauseMenu : BasePauseMenu
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

    protected override void OnOpened()
    {
        //PlayerCameraInput.Instance.enabled = false;
    }

    protected override void OnClosed()
    {
        //PlayerCameraInput.Instance.enabled = true;
    }

    public void OnContinueClicked()
    {
        Close();
    }

    public void OnRestartClicked()
    {
        playManager.RestartLevel();
        Close();
    }

    public void OnExitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

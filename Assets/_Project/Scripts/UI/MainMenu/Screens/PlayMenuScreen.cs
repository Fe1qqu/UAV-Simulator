using UnityEngine;
using UnityEngine.UI;

public class PlayMenuScreen : MainMenuScreenBase
{
    [SerializeField] private Button selectLevelButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (selectLevelButton == null)
        {
            Debug.LogError("[PlayMenuScreen] SelectLevelButton is not assigned.");
        }

        if (backButton == null)
        {
            Debug.LogError("[PlayMenuScreen] BackButton is not assigned.");
        }
    }

    protected override void OnShowInternal()
    {
        selectLevelButton.onClick.AddListener(OnSelectLevelClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    protected override void OnHideInternal()
    {
        selectLevelButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    private void OnSelectLevelClicked()
    {
        stateMachine.Show<LevelSelectScreen>(
            new LevelSelectContext(
                LevelSelectMode.Play,
                () => stateMachine.Show<PlayMenuScreen>()
            )
        );
    }

    private void OnBackClicked()
    {
        stateMachine.Show<MainMenuScreen>();
    }

    public override bool OnBack()
    {
        OnBackClicked();
        return true;
    }
}

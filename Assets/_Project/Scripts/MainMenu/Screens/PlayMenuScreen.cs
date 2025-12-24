using UnityEngine;
using UnityEngine.UI;

public class PlayMenuScreen : UIScreen, IBackHandler
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

    public override void OnShow()
    {
        selectLevelButton.onClick.AddListener(OnSelectLevel);
        backButton.onClick.AddListener(() =>
        {
            stateMachine.Show<MainMenuScreen>();
        });
    }

    public override void OnHide()
    {
        selectLevelButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    private void OnSelectLevel()
    {
        print("[PlayMenuScreen] OnSelectLevel");
        //stateMachine.Show<LevelSelectForPlayScreen>();
    }

    public bool OnBack()
    {
        stateMachine.Show<MainMenuScreen>();
        return true;
    }
}

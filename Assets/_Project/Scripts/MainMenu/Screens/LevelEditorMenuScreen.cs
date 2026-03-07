using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMenuScreen : MainMenuScreenBase
{
    [SerializeField] private Button createLevelButton;
    [SerializeField] private Button loadLevelButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (createLevelButton == null)
        {
            Debug.LogError("[LevelEditorMenuScreen] CreateLevelButton is not assigned.");
        }

        if (loadLevelButton == null)
        {
            Debug.LogError("[LevelEditorMenuScreen] LoadLevelButton is not assigned.");
        }

        if (backButton == null)
        {
            Debug.LogError("[LevelEditorMenuScreen] BackButton is not assigned.");
        }
    }

    protected override void OnShowInternal()
    {
        createLevelButton.onClick.AddListener(OnCreateClicked);
        loadLevelButton.onClick.AddListener(OnLoadClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    protected override void OnHideInternal()
    {
        createLevelButton.onClick.RemoveAllListeners();
        loadLevelButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    private void OnCreateClicked()
    {
        stateMachine.Show<LevelCreationWizardScreen>();
    }

    private void OnLoadClicked()
    {
        stateMachine.Show<LevelSelectScreen>(
            new LevelSelectContext(
                LevelSelectMode.LevelEditor,
                () => stateMachine.Show<LevelEditorMenuScreen>()
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

using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMenuScreen : UIScreen, IBackHandler
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

    public override void OnShow()
    {
        createLevelButton.onClick.AddListener(OnCreate);
        loadLevelButton.onClick.AddListener(OnLoad);
        backButton.onClick.AddListener(() =>
        {
            stateMachine.Show<MainMenuScreen>();
        });
    }

    public override void OnHide()
    {
        createLevelButton.onClick.RemoveAllListeners();
        loadLevelButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
    }

    private void OnCreate()
    {
        stateMachine.Show<LevelCreationWizardScreen>();
    }

    private void OnLoad()
    {
        print("[LevelEditorMenuScreen] OnLoad");
        //stateMachine.Show<LevelLoadScreen>();
    }

    public bool OnBack()
    {
        stateMachine.Show<MainMenuScreen>();
        return true;
    }
}

using UnityEngine;

public class LevelCreationWizardScreen : MainMenuScreenBase
{
    [SerializeField] private LevelCreationWizard levelCreationWizard;

    private void Awake()
    {
        if (levelCreationWizard == null)
        {
            Debug.LogError("[LevelCreationWizardScreen] LevelCreationWizard is not assigned.");
        }
    }

    protected override void OnShowInternal()
    {
        GameSettings.Instance.ClearEditorSession();

        levelCreationWizard.OnExit = ExitToEditorMenu;
        levelCreationWizard.OnExitToMainMenu = ExitToMainMenu;

        levelCreationWizard.StartWizard();
    }

    protected override void OnHideInternal()
    {
        levelCreationWizard.StopWizard();

        levelCreationWizard.OnExit = null;
        levelCreationWizard.OnExitToMainMenu = null;
    }

    private void ExitToEditorMenu()
    {
        stateMachine.Show<LevelEditorMenuScreen>();
    }

    private void ExitToMainMenu()
    {
        stateMachine.Show<MainMenuScreen>();
    }

    public override bool OnBack()
    {
        if (levelCreationWizard.GoBackOneStep())
        {
            return true;
        }

        ExitToEditorMenu();
        return true;
    }
}

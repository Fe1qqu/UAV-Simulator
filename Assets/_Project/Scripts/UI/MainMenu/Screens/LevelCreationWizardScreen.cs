using UnityEngine;

public class LevelCreationWizardScreen : MainMenuScreenBase, IBackHandler
{
    [SerializeField] private LevelCreationWizard levelCreationWizard;

    private void Awake()
    {
        if (levelCreationWizard == null)
        {
            Debug.LogError("[LevelCreationWizardScreen] LevelCreationWizard is not assigned.");
        }
    }

    public override void OnShow()
    {
        GameSettings.Instance.ClearEditorSession();

        levelCreationWizard.OnExit = ExitToEditorMenu;
        levelCreationWizard.OnExitToMainMenu = ExitToMainMenu;

        levelCreationWizard.StartWizard();
    }

    public override void OnHide()
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

    public bool OnBack()
    {
        if (levelCreationWizard.GoBackOneStep())
        {
            return true;
        }

        ExitToEditorMenu();
        return true;
    }
}

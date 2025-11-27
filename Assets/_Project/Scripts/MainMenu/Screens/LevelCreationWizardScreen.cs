using UnityEngine;

public class LevelCreationWizardScreen : UIScreen, IBackHandler
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
        levelCreationWizard.OnExitToMenu = () => stateMachine.Show<MainMenuScreen>();
        levelCreationWizard.StartWizard();
    }

    public override void OnHide()
    {
        levelCreationWizard.StopWizard();
        levelCreationWizard.OnExitToMenu = null;
    }

    public bool OnBack()
    {
        if (!levelCreationWizard.GoBackOneStep())
        {
            stateMachine.Show<MainMenuScreen>();
        }

        return true;
    }
}

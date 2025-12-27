using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : UIScreen, IBackHandler
{
    [SerializeField] private SettingsMenuController settingsMenuController;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (settingsMenuController == null)
        {
            Debug.LogError("[SettingsScreen] SettingsMenuController is not assigned.");
        }

        if (closeButton == null)
        {
            Debug.LogError("[SettingsScreen] CloseButton is not assigned.");
        }
    }

    public override void OnShow()
    {
        settingsMenuController.Show(SettingsContext.MainMenu);

        closeButton.onClick.AddListener(() =>
        {
            stateMachine.Show<MainMenuScreen>();
        });
    }

    public override void OnHide()
    {
        settingsMenuController.Hide();
        closeButton.onClick.RemoveAllListeners();
    }

    public bool OnBack()
    {
        stateMachine.Show<MainMenuScreen>();
        return true;
    }
}

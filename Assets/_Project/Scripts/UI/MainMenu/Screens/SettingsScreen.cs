using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MainMenuScreenBase, IBackHandler
{
    [SerializeField] private SettingsMenuController settingsMenuController;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (settingsMenuController == null)
        {
            Debug.LogError("[SettingsScreen] SettingsMenuController is not assigned.");
        }

        if (backButton == null)
        {
            Debug.LogError("[SettingsScreen] BackButton is not assigned.");
        }
    }

    public override void OnShow()
    {
        settingsMenuController.Show(SettingsContext.MainMenu);

        backButton.onClick.AddListener(OnBackClicked);
    }

    public override void OnHide()
    {
        settingsMenuController.Hide();

        backButton.onClick.RemoveAllListeners();
    }

    private void OnBackClicked()
    {
        stateMachine.Show<MainMenuScreen>();
    }

    public bool OnBack()
    {
        OnBackClicked();
        return true;
    }
}

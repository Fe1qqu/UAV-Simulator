using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MainMenuScreenBase
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

    protected override void OnShowInternal()
    {
        settingsMenuController.Show(SettingsContext.MainMenu);

        backButton.onClick.AddListener(OnBackClicked);
    }

    protected override void OnHideInternal()
    {
        settingsMenuController.Hide();

        backButton.onClick.RemoveAllListeners();
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

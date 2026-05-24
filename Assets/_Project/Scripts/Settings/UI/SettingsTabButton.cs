using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(Button))]
public class SettingsTabButton : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent labelLocalizationStringEvent;
    [SerializeField] private UISelectionButtonVisual visual;

    private Button button;
    private SettingsMenuController settingsMenuController;

    public string TabId { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();

        if (labelLocalizationStringEvent == null)
        {
            Debug.LogError("[SettingsTabButton] LabelLocalizationStringEvent is not assigned.");
        }

        if (visual == null)
        {
            Debug.LogError("[SettingsTabButton] Visual is not assigned.");
        }
    }

    public void Setup(string tabId, SettingsMenuController settingsMenuController)
    {
        TabId = tabId;
        this.settingsMenuController = settingsMenuController;

        if (labelLocalizationStringEvent != null)
        {
            labelLocalizationStringEvent.StringReference.TableEntryReference = tabId;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        SetSelected(false);
    }

    private void OnClicked()
    {
        if (settingsMenuController == null)
        {
            Debug.LogError("[SettingsTabButton] SettingsMenuController is null.");
            return;
        }

        settingsMenuController.SelectTab(TabId);
    }

    public void SetSelected(bool selected)
    {
        if (visual != null)
        {
            visual.SetSelected(selected);
        }
    }
}

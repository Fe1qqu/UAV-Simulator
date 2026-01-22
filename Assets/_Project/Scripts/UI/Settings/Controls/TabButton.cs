using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class TabButton : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent labelLocalizationStringEvent;

    private CanvasGroup canvasGroup;
    private Button button;
    private SettingsMenuController settingsMenuController;

    public string TabId { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (labelLocalizationStringEvent == null)
        {
            Debug.LogError("[TabButton] LabelLocalizationStringEvent is not assigned.");
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
            Debug.LogError("[TabButton] SettingsMenuController is null.");
        }

        settingsMenuController.SelectTab(TabId);
    }

    public void SetSelected(bool selected)
    {
        canvasGroup.alpha = selected ? 0.6f : 1.0f;
    }
}

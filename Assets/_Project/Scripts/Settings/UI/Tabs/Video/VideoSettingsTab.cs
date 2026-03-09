using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VideoSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private OptionSelectorSettingUI vSyncSettingUI;
    [SerializeField] private SliderSettingUI fpsLimitSettingUI;
    [SerializeField] private Button applyButton;

    protected override void Awake()
    {
        base.Awake();

        if (vSyncSettingUI == null)
        {
            Debug.LogError("[VideoSettingsTab] VSyncSettingUI is not assigned.");
        }

        if (fpsLimitSettingUI == null)
        {
            Debug.LogError("[VideoSettingsTab] FpsLimitSettingUI is not assigned.");
        }

        if (applyButton == null)
        {
            Debug.LogError("[VideoSettingsTab] ApplyButton is not assigned.");
        }

        vSyncSettingUI.Bind(GameSettings.Instance.Get("vsync"));
        fpsLimitSettingUI.Bind(GameSettings.Instance.Get("fps_limit"));

        applyButton.onClick.AddListener(ApplyTabSettings);
    }

    private void ApplyTabSettings()
    {
        var settings = new List<SettingInstance>();

        if (vSyncSettingUI.BoundSetting != null)
        {
            settings.Add(vSyncSettingUI.BoundSetting);
        }

        if (fpsLimitSettingUI.BoundSetting != null)
        {
            settings.Add(fpsLimitSettingUI.BoundSetting);
        }

        GameSettings.Instance.Save(settings);
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        applyButton.gameObject.SetActive(true);
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();

        applyButton.gameObject.SetActive(false);
    }

    public override void ResetTabState()
    {
        vSyncSettingUI.Reload();
        fpsLimitSettingUI.Reload();
    }

    private void OnDestroy()
    {
        applyButton.onClick.RemoveAllListeners();
    }
}

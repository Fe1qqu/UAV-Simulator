using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private Toggle vSyncToggle;

    protected override void Awake()
    {
        base.Awake();

        if (vSyncToggle == null)
        {
            Debug.LogWarning("[VideoSettingsTab] vSyncToggle is not assigned.");
        }
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        vSyncToggle.isOn = GameSettings.Instance.VSyncEnabled;
        vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();

        vSyncToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnVSyncChanged(bool enabled)
    {
        GameSettings.Instance.SetVSync(enabled);
    }
}

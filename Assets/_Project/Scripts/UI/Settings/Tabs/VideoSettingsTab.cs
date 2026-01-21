using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private Toggle vSyncToggle;

    private void Awake()
    {
        if (vSyncToggle == null)
        {
            Debug.LogWarning("[VideoSettingsTab] vSyncToggle not assigned.");
        }
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        //if (vSyncToggle != null)
        //{
        //    vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        //    vSyncToggle.onValueChanged.RemoveListener(OnToggleChanged);
        //    vSyncToggle.onValueChanged.AddListener(OnToggleChanged);
        //}
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();

        //if (vSyncToggle != null)
        //{
        //    vSyncToggle.onValueChanged.RemoveListener(OnToggleChanged);
        //}
    }

    private void OnToggleChanged(bool v)
    {
        //PlayerPrefs.SetInt("VSync", v ? 1 : 0);
        //PlayerPrefs.Save();
        Debug.Log($"[VideoSettingsTab] VSync = {v}");
    }
}

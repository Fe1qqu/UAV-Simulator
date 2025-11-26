using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Пример страницы с опцией специфичной для редактора.
/// </summary>
public class VideoSettingsPage : SettingsPage
{
    [Header("UI")]
    [SerializeField] private Toggle vSyncToggle;

    private void Awake()
    {
        if (vSyncToggle == null)
        {
            Debug.LogWarning("[EditorSettingsPage] vSyncToggle not assigned.");
        }
    }

    public override void OnPageSelected()
    {
        //base.OnPageSelected();

        //if (vSyncToggle != null)
        //{
        //    vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        //    vSyncToggle.onValueChanged.RemoveListener(OnToggleChanged);
        //    vSyncToggle.onValueChanged.AddListener(OnToggleChanged);
        //}
    }

    public override void OnPageUnselected()
    {
        //base.OnPageUnselected();

        //if (vSyncToggle != null)
        //{
        //    vSyncToggle.onValueChanged.RemoveListener(OnToggleChanged);
        //}
    }

    private void OnToggleChanged(bool v)
    {
        //PlayerPrefs.SetInt("VSync", v ? 1 : 0);
        //PlayerPrefs.Save();
        Debug.Log($"[EditorSettingsPage] VSync = {v}");
    }
}

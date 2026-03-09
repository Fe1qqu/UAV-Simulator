using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// Base class for the settings tab
/// </summary>
public abstract class SettingsTabBase : MonoBehaviour
{
    [Header("Identity")]
    public string tabId;

    [Header("Localization")]
    [SerializeField] private LocalizedString tabTitleLocalization;

    protected virtual void Awake()
    {
        if (string.IsNullOrWhiteSpace(tabId))
        {
            Debug.LogError($"[SettingsTabBase] TabId is empty on {name}.");
        }

        if (tabTitleLocalization == null)
        {
            Debug.LogError($"[SettingsTabBase] LocalizedString is not assigned on {name}.");
            return;
        }
    }

    /// <summary>
    /// Called when the tab becomes active (selected)
    /// </summary>
    public virtual void OnTabSelected()
    {
        //Debug.Log($"[SettingsTabBase] OnTabSelected: {TabId}.");
    }

    /// <summary>
    /// Called when the tab is hidden (switching to another tab)
    /// </summary>
    public virtual void OnTabUnselected()
    {
        //Debug.Log($"[SettingsTabBase] OnTabUnselected: {TabId}.");
    }

    public virtual void ResetTabState() { }
}

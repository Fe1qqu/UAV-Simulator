using UnityEngine;

/// <summary>
/// Base class for the settings tab
/// </summary>
public abstract class SettingsTabBase : MonoBehaviour
{
    [Tooltip("Unique tab identifier.")]
    [SerializeField] private string tabId;

    [Tooltip("Tab title.")]
    [SerializeField] private string displayName;

    public string TabId => tabId;
    public string DisplayName => displayName;

    /// <summary>
    /// Called when the tab becomes active (selected)
    /// </summary>
    public virtual void OnTabSelected()
    {
        Debug.Log($"[SettingsTab] OnTabSelected: {TabId}.");
    }

    /// <summary>
    /// Called when the tab is hidden (switching to another tab)
    /// </summary>
    public virtual void OnTabUnselected()
    {
        Debug.Log($"[SettingsTab] OnTabUnselected: {TabId}.");
    }
}

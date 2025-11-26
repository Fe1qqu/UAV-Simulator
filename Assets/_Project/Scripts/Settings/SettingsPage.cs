using UnityEngine;

/// <summary>
/// Base class for the settings page
/// </summary>
public abstract class SettingsPage : MonoBehaviour
{
    [Tooltip("Unique page identifier (e.g. audio, video).")]
    [SerializeField] private string pageId = "";
    [Tooltip("Page title visible in the tab.")]
    [SerializeField] private string displayName = "";

    public string PageId => pageId;
    public string DisplayName => displayName;

    /// <summary>
    /// Called when the page becomes active (selected)
    /// </summary>
    public virtual void OnPageSelected()
    {
        Debug.Log($"[SettingsPage] OnPageSelected: {PageId}.");
    }

    /// <summary>
    /// Called when the page is hidden (switching to another tab)
    /// </summary>
    public virtual void OnPageUnselected()
    {
        Debug.Log($"[SettingsPage] OnPageUnselected: {PageId}.");
    }
}

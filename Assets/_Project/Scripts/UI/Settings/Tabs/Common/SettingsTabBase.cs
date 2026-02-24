using UnityEngine;

/// <summary>
/// Base class for the settings tab
/// </summary>
public abstract class SettingsTabBase : MonoBehaviour
{
    [Header("Localization")]
    [SerializeField] private string localizationTableEntryKey;

    public string TabId => localizationTableEntryKey;

    protected virtual void Awake()
    {
        if (string.IsNullOrWhiteSpace(localizationTableEntryKey))
        {
            Debug.LogError($"[SettingsTabBase] LocalizationTableEntryKey is empty on {name}.");
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
}

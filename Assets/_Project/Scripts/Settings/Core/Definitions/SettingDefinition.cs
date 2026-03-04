using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

public enum SettingAutoApply
{
    None,

    /// <summary>
    /// Automatically applied during application boot (after GameSettings initialization).
    /// </summary>
    OnAppBoot,

    /// <summary>
    /// Automatically applied every time gameplay (Play/Editor) is entered.
    /// </summary>
    OnEnterGameplay
}

public abstract class SettingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string settingId;

    [Header("Localization")]
    public LocalizedString displayName;

    [Header("Auto Apply")]
    public SettingAutoApply autoApply = SettingAutoApply.None;

    [Header("Handler")]
    public SettingHandlerBase handler;

    [Header("Dependency Rules")]
    public List<SettingDependencyRule> dependencyRules = new();

    public abstract object GetValueFromStorage();
    public abstract void SaveValueToStorage(object value);
    public abstract object GetDefaultValue();
}

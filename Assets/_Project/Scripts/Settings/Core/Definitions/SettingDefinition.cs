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
    [SerializeField] private string id;
    public string Id => id;

    [Header("Localization")]
    [SerializeField] private LocalizedString displayName;
    public LocalizedString DisplayName => displayName;

    [Header("Auto Apply")]
    [SerializeField] private SettingAutoApply autoApply = SettingAutoApply.None;
    public SettingAutoApply AutoApply => autoApply;

    [Header("Handler")]
    [SerializeField] private SettingHandlerBase handler;
    public SettingHandlerBase Handler => handler;

    [Header("Dependency Rules")]
    [SerializeField] private List<SettingDependencyRule> dependencyRules = new();
    public IReadOnlyList<SettingDependencyRule> DependencyRules => dependencyRules;

    public abstract object GetValueFromStorage();
    public abstract void SaveValueToStorage(object value);
    public abstract object GetDefaultValue();
}

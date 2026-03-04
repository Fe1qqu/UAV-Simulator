using UnityEngine;
using System;
using System.Collections.Generic;

public class SettingInstance : IGameSetting
{
    public string Id => Definition.settingId;
    public SettingDefinition Definition { get; }

    public event Action<object> OnValueChanged;
    public event Action<bool> OnVisibilityChanged;

    private object currentValue;
    private readonly Dictionary<string, SettingInstance> dependencyTargets = new();

    public bool IsVisible { get; private set; } = true;

    public SettingInstance(SettingDefinition definition, Func<string, SettingInstance> resolver)
    {
        Definition = definition;
        Load();

        foreach (SettingDependencyRule rule in Definition.dependencyRules)
        {
            SettingDefinition targetDefinition = rule.condition.targetSetting;
            if (targetDefinition == null)
            {
                Debug.Log($"[SettingInstance] Dependency rule on '{Definition.name}' (ID: {Definition.settingId}) has null targetSetting in condition.");
                continue;
            }

            if (dependencyTargets.ContainsKey(targetDefinition.settingId))
            {
                continue;
            }

            SettingInstance targetInstance = resolver(targetDefinition.settingId);
            if (targetInstance == null)
            {
                Debug.Log($"[SettingInstance] Failed to resolve dependency target '{targetDefinition.name}' " +
                            $"(ID: {targetDefinition.settingId}) for setting '{Definition.name}' " +
                            $"(ID: {Definition.settingId}).");
                continue;
            }

            dependencyTargets.Add(targetDefinition.settingId, targetInstance);
            targetInstance.OnValueChanged += _ => EvaluateRules();
        }

        EvaluateRules();
    }

    public object GetValue() => currentValue;

    public void SetValue(object value)
    {
        if (Equals(currentValue, value))
        {
            //Debug.LogWarning("1");
            return;
        }

        currentValue = value;
        OnValueChanged?.Invoke(currentValue);
    }

    public void Apply()
    {
        Definition.handler.Apply(this);
    }

    public void Save()
    {
        Definition.SaveValueToStorage(currentValue);
    }

    public void Load()
    {
        currentValue = Definition.GetValueFromStorage();
    }

    private void SetVisibility(bool visible)
    {
        if (IsVisible == visible)
        {
            return;
        }

        IsVisible = visible;
        OnVisibilityChanged?.Invoke(IsVisible);
    }

    private void EvaluateRules()
    {
        //Debug.LogWarning($"[SettingInstance] {Definition.name} EvaluateRules.");

        bool visible = true;

        foreach (SettingDependencyRule rule in Definition.dependencyRules)
        {
            if (!dependencyTargets.TryGetValue(rule.condition.targetSetting.settingId, out var targetInstance))
            {
                continue;
            }

            bool condition = rule.condition.Evaluate(targetInstance.GetValue());

            if (!condition)
            {
                continue;
            }

            switch (rule.effect.effectType)
            {
                case SettingEffectType.Hide:
                    visible = false;
                    break;

                case SettingEffectType.ForceValue:
                    SetValue(rule.effect.forcedValue);
                    break;
            }
        }

        SetVisibility(visible);
    }
}

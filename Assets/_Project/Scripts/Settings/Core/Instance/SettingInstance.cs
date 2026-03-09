using UnityEngine;
using System;
using System.Collections.Generic;

public class SettingInstance : IGameSetting
{
    public string Id => Definition.settingId;
    public SettingDefinition Definition { get; }

    public event Action<object> OnRuntimeValueChanged;
    public event Action<bool> OnVisibilityChanged;

    private object runtimeValue;

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
            targetInstance.OnRuntimeValueChanged += _ => EvaluateRules();
        }

        EvaluateRules();
    }

    public object GetRuntimeValue() => runtimeValue;

    public void SetRuntimeValue(object value)
    {
        if (Equals(runtimeValue, value))
        {
            return;
        }

        runtimeValue = value;
        OnRuntimeValueChanged?.Invoke(runtimeValue);
    }

    public void Apply()
    {
        Definition.handler.Apply(this);
    }

    public void Save()
    {
        Definition.SaveValueToStorage(runtimeValue);
    }

    public void Load()
    {
        runtimeValue = Definition.GetValueFromStorage();
    }

    public void Reload()
    {
        Load();
        OnRuntimeValueChanged?.Invoke(runtimeValue);
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

            bool condition = rule.condition.Evaluate(targetInstance.runtimeValue);

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
                    SetRuntimeValue(rule.effect.forcedValue);
                    break;
            }
        }

        SetVisibility(visible);
    }
}

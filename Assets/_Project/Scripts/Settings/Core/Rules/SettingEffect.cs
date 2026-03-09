using Alchemy.Inspector;
using System;

public enum SettingEffectType
{
    Hide,
    ForceValue
}

[Serializable]
public class SettingEffect
{
    public SettingEffectType effectType;

    [ShowIf(nameof(IsForceValue))]
    public float forcedValue;

    private bool IsForceValue()
    {
        return effectType == SettingEffectType.ForceValue;
    }
}

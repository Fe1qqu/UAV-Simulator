using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;

[CreateAssetMenu(menuName = "Game Settings/Range Setting")]
public class RangeSettingDefinition : SettingDefinition
{
    [Header("Range")]
    public float minValue = 0f;
    public float maxValue = 1f;
    public float step = 0.1f;

    [Header("Default")]
    public float defaultValue = 0f;

    [Header("Special Values")]
    public bool hasSpecialMinValue;

    [ShowIf(nameof(hasSpecialMinValue))]
    public LocalizedString specialMinValueLabel;

    public bool hasSpecialMaxValue;

    [ShowIf(nameof(hasSpecialMaxValue))]
    public LocalizedString specialMaxValueLabel;

    public override object GetValueFromStorage()
    {
        return PlayerPrefs.GetFloat(settingId, defaultValue);
    }

    public override void SaveValueToStorage(object value)
    {
        PlayerPrefs.SetFloat(settingId, (float)value);
    }

    public override object GetDefaultValue() => defaultValue;
}

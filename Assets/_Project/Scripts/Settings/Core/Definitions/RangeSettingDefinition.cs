using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;

[CreateAssetMenu(menuName = "Game Settings/Range Setting")]
public class RangeSettingDefinition : SettingDefinition
{
    [Header("Range")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private float step = 0.1f;

    public float MinValue => minValue;
    public float MaxValue => maxValue;
    public float Step => step;

    [Header("Default")]
    [SerializeField] private float defaultValue = 0f;

    [Header("Special Values")]
    [SerializeField] private bool hasSpecialMinValue;
    public bool HasSpecialMinValue => hasSpecialMinValue;

    [ShowIf(nameof(hasSpecialMinValue))]
    [SerializeField] private LocalizedString specialMinValueLabel;
    public LocalizedString SpecialMinValueLabel => specialMinValueLabel;

    [SerializeField] private bool hasSpecialMaxValue;
    public bool HasSpecialMaxValue => hasSpecialMaxValue;

    [ShowIf(nameof(hasSpecialMaxValue))]
    [SerializeField] private LocalizedString specialMaxValueLabel;
    public LocalizedString SpecialMaxValueLabel => specialMaxValueLabel;

    public override object GetValueFromStorage()
    {
        return PlayerPrefs.GetFloat(Id, defaultValue);
    }

    public override void SaveValueToStorage(object value)
    {
        PlayerPrefs.SetFloat(Id, (float)value);
    }

    public override object GetDefaultValue() => defaultValue;
}

using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[System.Serializable]
public class SettingOption
{
    public string valueId;
    public LocalizedString localizedLabel;
}

[CreateAssetMenu(menuName = "Game Settings/Option Setting")]
public class OptionSettingDefinition : SettingDefinition
{
    [SerializeField] private List<SettingOption> options = new();
    public IReadOnlyList<SettingOption> Options => options;

    [Header("Default")]
    [SerializeField] private int defaultIndex = 0;

    public override object GetValueFromStorage()
    {
        return PlayerPrefs.GetInt(Id, defaultIndex);
    }

    public override void SaveValueToStorage(object value)
    {
        PlayerPrefs.SetInt(Id, (int)value);
    }

    public override object GetDefaultValue() => defaultIndex;
}

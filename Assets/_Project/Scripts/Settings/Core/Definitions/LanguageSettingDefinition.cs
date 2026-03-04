using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Settings/Language Setting")]
public class LanguageSettingDefinition : SettingDefinition
{
    [SerializeField] private List<Locale> availableLocales = new();
    [SerializeField] private Locale defaultLocale;

    public IReadOnlyList<Locale> AvailableLocales => availableLocales;

    public override object GetValueFromStorage()
    {
        string code = PlayerPrefs.GetString(settingId, defaultLocale.name);
        Locale locale = availableLocales.Find(locale => locale.name == code);
        return locale != null ? locale : defaultLocale;
    }

    public override void SaveValueToStorage(object value)
    {
        if (value is Locale locale)
        {
            PlayerPrefs.SetString(settingId, locale.name);
        }
    }

    public override object GetDefaultValue() => defaultLocale;
}

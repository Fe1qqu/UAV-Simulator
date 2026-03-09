using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Settings/Language Setting")]
public class LanguageSettingDefinition : SettingDefinition
{
    [SerializeField] private List<Locale> availableLocales = new();
    public IReadOnlyList<Locale> AvailableLocales => availableLocales;

    [Header("Default")]
    [SerializeField] private Locale defaultLocale;

    public override object GetValueFromStorage()
    {
        string code = PlayerPrefs.GetString(Id, defaultLocale.name);
        Locale locale = availableLocales.Find(locale => locale.name == code);
        return locale != null ? locale : defaultLocale;
    }

    public override void SaveValueToStorage(object value)
    {
        if (value is Locale locale)
        {
            PlayerPrefs.SetString(Id, locale.name);
        }
    }

    public override object GetDefaultValue() => defaultLocale;
}

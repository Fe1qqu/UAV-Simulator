using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(menuName = "Game Settings/Handlers/Language")]
public class LanguageHandler : SettingHandlerBase
{
    public override async void Apply(SettingInstance setting)
    {
        Locale locale = setting.GetRuntimeValue() as Locale;
        if (locale == null)
        {
            return;
        }

        await LocalizationSettings.InitializationOperation.Task;

        LocalizationSettings.SelectedLocale = locale;
    }
}

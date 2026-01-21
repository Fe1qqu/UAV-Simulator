using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using TMPro;

public class LanguageSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private readonly List<Locale> locales = new();
    private bool isInitialized;

    private void Awake()
    {
        if (languageDropdown == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] LanguageDropdown is not assigned.");
        }
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        if (!isInitialized)
        {
            InitializeDropdown();
            isInitialized = true;
        }

        SyncSelection();
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();

        languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
    }

    private void InitializeDropdown()
    {
        locales.Clear();
        languageDropdown.ClearOptions();

        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            locales.Add(locale);
        }

        List<string> options = new();
        foreach (Locale locale in locales)
        {
            options.Add(locale.LocaleName);
        }

        languageDropdown.AddOptions(options);
    }

    private void SyncSelection()
    {
        int index = locales.IndexOf(LocalizationSettings.SelectedLocale);
        if (index >= 0)
        {
            languageDropdown.SetValueWithoutNotify(index);
        }
    }

    private void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= locales.Count)
        {
            return;
        }

        Locale selected = locales[index];
        GameSettings.Instance.SetLocale(selected);
    }
}

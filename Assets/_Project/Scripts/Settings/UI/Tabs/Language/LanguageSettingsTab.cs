using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using System.Collections.Generic;

public class LanguageSettingsTab : SettingsTabBase
{
    [Header("Reference")]
    [SerializeField] private LanguageSettingDefinition definitionReference;

    [Header("UI")]
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject languageButtonPrefab;

    private LanguageSettingDefinition languageDefinition;
    private SettingInstance languageSetting;

    private readonly List<(Button button, UISelectionButtonVisual visual, Locale locale)> languageButtons = new();

    protected override void Awake()
    {
        base.Awake();

        if (definitionReference == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] DefinitionReference is not assigned.");
        }

        if (buttonsContainer == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] ButtonsContainer is not assigned.");
        }

        if (languageButtonPrefab == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] LanguageButtonPrefab is not assigned.");
        }

        languageSetting = GameSettings.Instance.Get(definitionReference.Id);
        languageDefinition = languageSetting.Definition as LanguageSettingDefinition;
        if (languageDefinition == null)
        {
            Debug.LogError("[LanguageSettingsTab] LanguageDefinition not found.");
            return;
        }

        BuildButtons();
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();
        UpdateButtonsVisual();
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();
    }

    private void BuildButtons()
    {
        if (languageButtons.Count > 0)
        {
            UpdateButtonsVisual();
            return;
        }

        foreach (Locale locale in languageDefinition.AvailableLocales)
        {
            GameObject buttonInstance = Instantiate(languageButtonPrefab, buttonsContainer);
            buttonInstance.SetActive(true);

            if (!buttonInstance.TryGetComponent(out Button button))
            {
                Debug.LogError("[LanguageSettingsTab] Button component not found.");
                continue;
            }

            if (!buttonInstance.TryGetComponent(out UISelectionButtonVisual visual))
            {
                Debug.LogError("[LanguageSettingsTab] UISelectionButtonVisual component not found.");
                continue;
            }

            TMP_Text label = buttonInstance.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = locale.LocaleName;
            }

            button.onClick.AddListener(() => OnLanguageSelected(locale));

            languageButtons.Add((button, visual, locale));
        }

        UpdateButtonsVisual();
    }

    private void OnLanguageSelected(Locale locale)
    {
        GameSettings.Instance.ChangeApplyAndSave(languageSetting, locale);
        UpdateButtonsVisual();
    }

    private void UpdateButtonsVisual()
    {
        Locale currentLocale = languageSetting.GetRuntimeValue() as Locale;

        foreach (var (_, visual, locale) in languageButtons)
        {
            bool selected = locale == currentLocale;
            visual.SetSelected(selected);
        }
    }
}

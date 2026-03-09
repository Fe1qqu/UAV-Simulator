using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using System.Collections.Generic;

public class LanguageSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject languageButtonPrefab;

    private LanguageSettingDefinition definition;
    private SettingInstance languageSetting;

    private readonly List<Button> languageButtons = new();

    protected override void Awake()
    {
        base.Awake();

        if (buttonsContainer == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] ButtonsContainer is not assigned.");
        }

        if (languageButtonPrefab == null)
        {
            Debug.LogWarning("[LanguageSettingsTab] LanguageButtonPrefab is not assigned.");
        }

        languageSetting = GameSettings.Instance.Get("language");
        definition = languageSetting.Definition as LanguageSettingDefinition;
        if (definition == null)
        {
            Debug.LogError("[LanguageSettingsTab] LanguageSettingDefinition not found.");
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

        Locale currentLocale = languageSetting.GetRuntimeValue() as Locale;
        
        foreach (Locale locale in definition.AvailableLocales)
        {
            GameObject buttonInstance = Instantiate(languageButtonPrefab, buttonsContainer);
            buttonInstance.SetActive(true);

            Button button = buttonInstance.GetComponent<Button>();
            TMP_Text label = buttonInstance.GetComponentInChildren<TMP_Text>();
            label.text = locale.LocaleName;

            button.onClick.AddListener(() => OnLanguageSelected(locale));

            languageButtons.Add(button);
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

        foreach (Button button in languageButtons)
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            bool isSelected = label.text == currentLocale.LocaleName;
            SetVisualState(button.gameObject, isSelected);
        }
    }

    private void SetVisualState(GameObject buttonObject, bool selected)
    {
        Transform highlight = buttonObject.transform.Find("SelectionHighlight");
        if (highlight == null)
        {
            Debug.LogError($"[LanguageSettingsTab] SelectionHighlight not found on button '{buttonObject.name}'.");
            return;
        }

        highlight.gameObject.SetActive(selected);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionSelectorSettingUI : SettingUIElementBase
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text valueLabel;

    [Header("Indicators")]
    [SerializeField] private Transform indicatorsContainer;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Color indicatorSelectedColor = Color.white;
    [SerializeField] private Color indicatorUnselectedColor = Color.lightGray;

    private OptionSettingDefinition optionDefinition;
    private SettingOption currentOption;
    private int currentIndex;

    protected override void Awake()
    {
        base.Awake();

        if (leftButton == null)
        {
            Debug.LogError($"[OptionSelectorSettingUI] LeftButton is not assigned.");
        }

        if (rightButton == null)
        {
            Debug.LogError($"[OptionSelectorSettingUI] RightButton is not assigned.");
        }

        if (valueLabel == null)
        {
            Debug.LogError($"[OptionSelectorSettingUI] ValueLabel is not assigned.");
        }

        if (indicatorsContainer == null)
        {
            Debug.LogError($"[OptionSelectorSettingUI] IndicatorsContainer is not assigned.");
        }

        if (indicatorPrefab == null)
        {
            Debug.LogError($"[OptionSelectorSettingUI] IndicatorPrefab is not assigned.");
        }
    }

    public override void Bind(SettingInstance setting)
    {
        base.Bind(setting);

        optionDefinition = definition as OptionSettingDefinition;
        if (optionDefinition == null)
        {
            Debug.LogError("[OptionSelectorSettingUI] Wrong definition type.");
            return;
        }

        leftButton.onClick.AddListener(SelectPrevious);
        rightButton.onClick.AddListener(SelectNext);

        BuildIndicators();
        RefreshFromModel();
    }

    private void RefreshFromModel()
    {
        currentIndex = (int)boundSetting.GetValue();
        UpdateUI();
    }

    protected override void OnSettingValueChanged(object value)
    {
        currentIndex = (int)value;
        UpdateUI();
    }

    private void SelectNext()
    {
        if (currentIndex >= optionDefinition.Options.Count - 1)
        {
            return;
        }

        GameSettings.Instance.SetValue(boundSetting, ++currentIndex);
    }

    private void SelectPrevious()
    {
        if (currentIndex <= 0)
        {
            return;
        }

        GameSettings.Instance.SetValue(boundSetting, --currentIndex);
    }

    private void UpdateUI()
    {
        UpdateValueLabel();
        UpdateIndicators();
    }

    private void UpdateValueLabel()
    {
        if (currentIndex < 0 || currentIndex >= optionDefinition.Options.Count)
        {
            return;
        }

        if (currentOption != null && currentOption.localizedLabel != null)
        {
            currentOption.localizedLabel.StringChanged -= OnValueLocalized;
        }

        currentOption = optionDefinition.Options[currentIndex];

        if (currentOption.localizedLabel != null)
        {
            currentOption.localizedLabel.StringChanged += OnValueLocalized;
            valueLabel.text = currentOption.localizedLabel.GetLocalizedString();
        }
        else
        {
            Debug.LogError($"[OptionSelectorSettingUI] CurrentOption.localizedLabel in null.");
        }
    }

    private void OnValueLocalized(string value)
    {
        valueLabel.text = value;
    }

    private void BuildIndicators()
    {
        foreach (Transform indicator in indicatorsContainer)
        {
            Destroy(indicator.gameObject);
        }

        for (int i = 0; i < optionDefinition.Options.Count; i++)
        {
            GameObject indicatorInstance = Instantiate(indicatorPrefab, indicatorsContainer);
            indicatorInstance.SetActive(true);
        }

        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        for (int i = 0; i < indicatorsContainer.childCount; i++)
        {
            if (!indicatorsContainer.GetChild(i).TryGetComponent<Image>(out var image))
            {
                continue;
            }

            image.color = (i == currentIndex) ? indicatorSelectedColor : indicatorUnselectedColor;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (currentOption != null && currentOption.localizedLabel != null)
        {
            currentOption.localizedLabel.StringChanged -= OnValueLocalized;
        }

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();
    }
}

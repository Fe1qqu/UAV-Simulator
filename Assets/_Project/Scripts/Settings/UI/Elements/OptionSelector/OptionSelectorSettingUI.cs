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

    protected override void Bind(SettingInstance setting)
    {
        base.Bind(setting);

        optionDefinition = boundSetting.Definition as OptionSettingDefinition;
        if (optionDefinition == null)
        {
            Debug.LogError("[OptionSelectorSettingUI] Wrong definition type.");
            return;
        }

        leftButton.onClick.AddListener(SelectPrevious);
        rightButton.onClick.AddListener(SelectNext);

        BuildIndicators();
        Refresh();
    }

    protected override void Refresh()
    {
        UpdateUI();
    }

    private void SelectNext()
    {
        int index = (int)boundSetting.GetRuntimeValue();

        if (index >= optionDefinition.Options.Count - 1)
        {
            return;
        }

        boundSetting.SetRuntimeValue(index + 1);
    }

    private void SelectPrevious()
    {
        int index = (int)boundSetting.GetRuntimeValue();

        if (index <= 0)
        {
            return;
        }

        boundSetting.SetRuntimeValue(index - 1);
    }

    private void UpdateUI()
    {
        int index = (int)boundSetting.GetRuntimeValue();

        if (index < 0 || index >= optionDefinition.Options.Count)
        {
            return;
        }

        UpdateValueLabel(index);
        UpdateIndicators(index);
    }

    private void UpdateValueLabel(int index)
    {
        if (currentOption != null && currentOption.localizedLabel != null)
        {
            currentOption.localizedLabel.StringChanged -= OnValueLocalized;
        }

        currentOption = optionDefinition.Options[index];

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
    }

    private void UpdateIndicators(int index)
    {
        for (int i = 0; i < indicatorsContainer.childCount; i++)
        {
            if (!indicatorsContainer.GetChild(i).TryGetComponent<Image>(out var image))
            {
                continue;
            }

            image.color = (i == index) ? indicatorSelectedColor : indicatorUnselectedColor;
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

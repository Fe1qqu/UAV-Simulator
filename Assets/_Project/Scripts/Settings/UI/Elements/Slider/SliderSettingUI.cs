using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

public class SliderSettingUI : SettingUIElementBase
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueLabel;

    private RangeSettingDefinition rangeDefinition;
    private LocalizedString activeLocalizedValue;

    protected override void Awake()
    {
        base.Awake();

        if (slider == null)
        {
            Debug.LogError($"[SliderSettingUI] Slider is not assigned.");
        }

        if (valueLabel == null)
        {
            Debug.LogError($"[SliderSettingUI] ValueLabel is not assigned.");
        }
    }

    public override void Bind(SettingInstance setting)
    {
        base.Bind(setting);

        rangeDefinition = definition as RangeSettingDefinition;
        if (rangeDefinition == null)
        {
            Debug.LogError("[SliderSettingUI] Wrong definition type.");
            return;
        }

        slider.minValue = rangeDefinition.minValue;
        slider.maxValue = rangeDefinition.maxValue;

        slider.onValueChanged.AddListener(OnSliderChanged);

        Refresh();
    }

    protected override void Unbind()
    {
        base.Unbind();

        slider.onValueChanged.RemoveListener(OnSliderChanged);
        SetActiveLocalizedValue(null);
    }

    protected override void Refresh()
    {
        float value = (float)boundSetting.GetRuntimeValue();
        float quantized = Quantize(value);

        slider.SetValueWithoutNotify(quantized);
        UpdateValueLabel(quantized);
    }

    private float Quantize(float rawValue)
    {
        float min = rangeDefinition.minValue;
        float max = rangeDefinition.maxValue;
        float step = rangeDefinition.step;

        float normalized = rawValue - min;
        float steps = Mathf.Round(normalized / step);
        float quantized = min + steps * step;

        return Mathf.Clamp(quantized, min, max);
    }

    private void UpdateValueLabel(float value)
    {
        LocalizedString targetLocalized = null;

        if (rangeDefinition.hasSpecialMinValue && value == rangeDefinition.minValue)
        {
            targetLocalized = rangeDefinition.specialMinValueLabel;
        }
        else if (rangeDefinition.hasSpecialMaxValue && value == rangeDefinition.maxValue)
        {
            targetLocalized = rangeDefinition.specialMaxValueLabel;
        }

        SetActiveLocalizedValue(targetLocalized);

        if (targetLocalized == null)
        {
            valueLabel.text = FormatValue(value);
        }
        else
        {
            valueLabel.text = targetLocalized.GetLocalizedString();
        }
    }

    private string FormatValue(float value)
    {
        float step = rangeDefinition.step;

        if (step >= 1f)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        int decimals = Mathf.Clamp(Mathf.CeilToInt(-Mathf.Log10(step)), 0, 6);

        return value.ToString($"F{decimals}");
    }

    private void SetActiveLocalizedValue(LocalizedString newValue)
    {
        if (activeLocalizedValue == newValue)
        {
            return;
        }

        if (activeLocalizedValue != null)
        {
            activeLocalizedValue.StringChanged -= OnSpecialValueChanged;
        }

        activeLocalizedValue = newValue;

        if (activeLocalizedValue != null)
        {
            activeLocalizedValue.StringChanged += OnSpecialValueChanged;
        }
    }

    private void OnSpecialValueChanged(string localizedText)
    {
        valueLabel.text = localizedText;
    }

    private void OnSliderChanged(float rawValue)
    {
        float quantized = Quantize(rawValue);

        slider.SetValueWithoutNotify(quantized);

        boundSetting.SetRuntimeValue(quantized);
    }
}

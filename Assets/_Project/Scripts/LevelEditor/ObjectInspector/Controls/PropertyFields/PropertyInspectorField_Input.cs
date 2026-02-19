using UnityEngine;
using TMPro;

public class PropertyInspectorField_Input : PropertyInspectorFieldBase
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_InputField valueInputField;

    private void Awake()
    {
        if (keyText == null)
        {
            Debug.LogError("[PropertyInspectorField_Input] KeyText is not assigned.");
        }

        if (valueInputField == null)
        {
            Debug.LogError("[PropertyInspectorField_Input] ValueInputField is not assigned.");
        }
    }

    public override void Bind(LevelObject levelObject, ObjectPropertyDefinition propertyDefinition)
    {
        if (boundObject != null)
        {
            boundObject.PropertyChanged -= OnPropertyChanged;
        }

        base.Bind(levelObject, propertyDefinition);

        keyText.text = boundPropertyDefinition.localizedString.GetLocalizedString();

        valueInputField.onValueChanged.RemoveAllListeners();
        valueInputField.onEndEdit.RemoveAllListeners();

        valueInputField.onValueChanged.AddListener(OnValueChanged);
        valueInputField.onEndEdit.AddListener(OnEndEdit);

        RefreshFromModel();
    }

    protected override void ApplyValueToUI(string value)
    {
        valueInputField.text = value;
    }

    private void OnValueChanged(string newValue)
    {
        if (suppressNotify)
        {
            return;
        }

        if (boundPropertyDefinition.type == ObjectPropertyType.Int)
        {
            bool allowNegative = !boundPropertyDefinition.useMin || boundPropertyDefinition.min < 0f;
            string filteredValue = NumericInputFilter.FilterInt(newValue, allowNegative);
            ApplyFiltered(filteredValue);
        }
        else if (boundPropertyDefinition.type == ObjectPropertyType.Float)
        {
            bool allowNegative = !boundPropertyDefinition.useMin || boundPropertyDefinition.min < 0f;
            string filteredValue = NumericInputFilter.FilterFloat(newValue, allowNegative);
            ApplyFiltered(filteredValue);
        }
    }

    private void ApplyFiltered(string filteredValue)
    {
        if (filteredValue == valueInputField.text)
        {
            return;
        }

        suppressNotify = true;
        ApplyValueToUI(filteredValue);
        suppressNotify = false;
    }

    private void OnEndEdit(string value)
    {
        if (suppressNotify)
        {
            return;
        }

        if (!validator.TryValidate(value, boundPropertyDefinition, out string normalizedValue, out string _))
        {
            return;
        }

        SetValue(normalizedValue);
    }
}

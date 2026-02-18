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

        keyText.text = propertyDefinition.localizedString.GetLocalizedString();
        valueInputField.text = GetCurrentValue();

        valueInputField.onValueChanged.RemoveAllListeners();
        valueInputField.onEndEdit.RemoveAllListeners();

        valueInputField.onValueChanged.AddListener(OnValueChanged);
        valueInputField.onEndEdit.AddListener(OnEndEdit);

        boundObject.PropertyChanged += OnPropertyChanged;
    }

    private void OnValueChanged(string newValue)
    {
        if (suppressNotify)
        {
            return;
        }

        if (propertyDefinition.type == ObjectPropertyType.Int)
        {
            bool allowNegative = !propertyDefinition.useMin || propertyDefinition.min < 0f;
            string filteredValue = NumericInputFilter.FilterInt(newValue, allowNegative);
            ApplyFiltered(filteredValue);
        }
        else if (propertyDefinition.type == ObjectPropertyType.Float)
        {
            bool allowNegative = !propertyDefinition.useMin || propertyDefinition.min < 0f;
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
        valueInputField.text = filteredValue;
        suppressNotify = false;
    }

    private void OnEndEdit(string value)
    {
        if (suppressNotify)
        {
            return;
        }

        if (!validator.TryValidate(value, propertyDefinition, out string normalizedValue, out string _))
        {
            return;
        }

        SetValue(normalizedValue);

        suppressNotify = true;
        valueInputField.text = normalizedValue;
        suppressNotify = false;
    }

    private void OnPropertyChanged(LevelObject _, string changedKey)
    {
        if (changedKey != propertyDefinition.key)
        {
            return;
        }

        suppressNotify = true;
        valueInputField.text = GetCurrentValue();
        suppressNotify = false;
    }

    private void OnDestroy()
    {
        if (boundObject != null)
        {
            boundObject.PropertyChanged -= OnPropertyChanged;
        }
    }
}

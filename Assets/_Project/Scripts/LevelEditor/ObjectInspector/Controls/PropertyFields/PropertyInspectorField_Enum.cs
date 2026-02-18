using UnityEngine;
using TMPro;

public class PropertyInspectorField_Enum : PropertyInspectorFieldBase
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Dropdown valueDropdown;

    private void Awake()
    {
        if (keyText == null)
        {
            Debug.LogError("[PropertyInspectorField_Enum] KeyText is not assigned.");
        }

        if (valueDropdown == null)
        {
            Debug.LogError("[PropertyInspectorField_Enum] ValueDropdown is not assigned.");
        }
    }

    public override void Bind(LevelObject levelObject, ObjectPropertyDefinition propertyDefinition)
    {
        if (propertyDefinition.enumOptions == null || propertyDefinition.enumOptions.Count == 0)
        {
            Debug.LogError($"[PropertyInspectorField_Enum] No enum options defined for key '{propertyDefinition.key}'.");
            return;
        }

        if (boundObject != null)
        {
            boundObject.PropertyChanged -= OnPropertyChanged;
        }

        base.Bind(levelObject, propertyDefinition);

        keyText.text = propertyDefinition.localizedString.GetLocalizedString();

        valueDropdown.ClearOptions();
        valueDropdown.AddOptions(propertyDefinition.enumOptions);

        int index = propertyDefinition.enumOptions.IndexOf(GetCurrentValue());
        valueDropdown.value = index >= 0 ? index : 0;

        valueDropdown.onValueChanged.RemoveAllListeners();
        valueDropdown.onValueChanged.AddListener(OnValueChanged);

        boundObject.PropertyChanged += OnPropertyChanged;
    }

    private void OnValueChanged(int index)
    {
        if (suppressNotify)
        {
            return;
        }

        SetValue(propertyDefinition.enumOptions[index]);
    }

    private void OnPropertyChanged(LevelObject _, string changedKey)
    {
        if (changedKey != propertyDefinition.key)
        {
            return;
        }

        suppressNotify = true;

        int index = propertyDefinition.enumOptions.IndexOf(GetCurrentValue());
        valueDropdown.value = index >= 0 ? index : 0;

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

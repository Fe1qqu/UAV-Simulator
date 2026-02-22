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

    public override void Bind(LevelObject levelObject, ObjectPropertyDefinition objectProperty)
    {
        if (objectProperty.enumOptions == null || objectProperty.enumOptions.Count == 0)
        {
            Debug.LogError($"[PropertyInspectorField_Enum] No enum options defined for key '{objectProperty.key}'.");
            return;
        }

        if (boundObject != null)
        {
            boundObject.PropertyChanged -= OnPropertyChanged;
        }

        base.Bind(levelObject, objectProperty);

        keyText.text = boundObjectProperty.localizedString.GetLocalizedString();

        valueDropdown.ClearOptions();
        valueDropdown.AddOptions(boundObjectProperty.enumOptions);

        valueDropdown.onValueChanged.RemoveAllListeners();
        valueDropdown.onValueChanged.AddListener(OnValueChanged);

        RefreshFromModel();
    }

    protected override void ApplyValueToUI(string value)
    {
        int index = boundObjectProperty.enumOptions.IndexOf(value);
        valueDropdown.value = index >= 0 ? index : 0;
    }

    private void OnValueChanged(int index)
    {
        if (suppressNotify)
        {
            return;
        }

        SetValue(boundObjectProperty.enumOptions[index]);
    }
}

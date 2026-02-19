using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyInspectorField_Bool : PropertyInspectorFieldBase
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Toggle valueToggle;

    private void Awake()
    {
        if (keyText == null)
        {
            Debug.LogError("[PropertyInspectorField_Bool] KeyText is not assigned.");
        }

        if (valueToggle == null)
        {
            Debug.LogError("[PropertyInspectorField_Bool] ValueToggle is not assigned.");
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

        valueToggle.onValueChanged.RemoveAllListeners();
        valueToggle.onValueChanged.AddListener(OnValueChanged);

        RefreshFromModel();
    }

    protected override void ApplyValueToUI(string value)
    {
        bool.TryParse(value, out bool parsed);
        valueToggle.isOn = parsed;
    }

    private void OnValueChanged(bool newValue)
    {
        if (suppressNotify)
        {
            return;
        }

        SetValue(newValue.ToString());
    }
}

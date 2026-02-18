using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        keyText.text = propertyDefinition.localizedString.GetLocalizedString();

        bool.TryParse(GetCurrentValue(), out bool value);
        valueToggle.isOn = value;

        valueToggle.onValueChanged.RemoveAllListeners();
        valueToggle.onValueChanged.AddListener(OnValueChanged);

        boundObject.PropertyChanged += OnPropertyChanged;
    }

    private void OnValueChanged(bool newValue)
    {
        if (suppressNotify)
        {
            return;
        }

        SetValue(newValue ? "true" : "false");
    }

    private void OnPropertyChanged(LevelObject _, string changedKey)
    {
        if (changedKey != propertyDefinition.key)
        {
            return;
        }

        suppressNotify = true;

        bool.TryParse(GetCurrentValue(), out bool value);
        valueToggle.isOn = value;

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

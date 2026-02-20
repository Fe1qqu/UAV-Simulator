using UnityEngine;

public abstract class PropertyInspectorFieldBase : MonoBehaviour
{
    protected LevelObject boundObject;
    protected ObjectPropertyDefinition boundObjectProperty;
    protected PropertyKey propertyKey;
    protected bool suppressNotify;

    protected IPropertyValueValidator validator = new DefaultPropertyValueValidator();

    public virtual void Bind(LevelObject levelObject, ObjectPropertyDefinition objectProperty)
    {
        boundObject = levelObject;
        boundObjectProperty = objectProperty;

        propertyKey = PropertyKeyRegistry.Get(boundObjectProperty.key);

        if (propertyKey == null)
        {
            Debug.LogError($"[PropertyInspectorFieldBase] PropertyKey '{boundObjectProperty.key}' is not registered.");
            return;
        }

        boundObject.PropertyChanged += OnPropertyChanged;

        RefreshFromModel();
    }

    protected virtual void OnDestroy()
    {
        if (boundObject != null)
        {
            boundObject.PropertyChanged -= OnPropertyChanged;
        }
    }

    protected virtual void RefreshFromModel()
    {
        suppressNotify = true;
        ApplyValueToUI(GetCurrentValue());
        suppressNotify = false;
    }

    protected string GetCurrentValue()
    {
        return boundObject.Get(propertyKey) ?? boundObjectProperty.defaultValue;
    }

    protected void SetValue(string newValue)
    {
        boundObject.Set(propertyKey, newValue);
    }

    protected virtual void OnPropertyChanged(LevelObject _, PropertyKey changedKey)
    {
        if (changedKey != propertyKey)
        {
            return;
        }

        RefreshFromModel();
    }

    protected abstract void ApplyValueToUI(string value);
}

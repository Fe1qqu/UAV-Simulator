using UnityEngine;

public abstract class PropertyInspectorFieldBase : MonoBehaviour
{
    protected LevelObject boundObject;
    protected ObjectPropertyDefinition propertyDefinition;
    protected bool suppressNotify;

    protected IPropertyValueValidator validator = new DefaultPropertyValueValidator();

    public virtual void Bind(LevelObject levelObject, ObjectPropertyDefinition propertyDefinition)
    {
        boundObject = levelObject;
        this.propertyDefinition = propertyDefinition;
    }

    protected string GetCurrentValue()
    {
        return boundObject.GetPropertyValue(propertyDefinition.key) ?? propertyDefinition.defaultValue;
    }

    protected void SetValue(string newValue)
    {
        boundObject.TrySetProperty(propertyDefinition.key, newValue);
    }
}

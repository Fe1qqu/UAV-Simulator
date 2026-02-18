public interface IPropertyValueValidator
{
    bool TryValidate(string input, ObjectPropertyDefinition objectProperty, out string normalizedValue, out string errorMessage);
}

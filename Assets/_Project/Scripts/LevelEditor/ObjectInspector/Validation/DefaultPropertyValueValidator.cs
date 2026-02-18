using UnityEngine;
using System.Globalization;

public class DefaultPropertyValueValidator : IPropertyValueValidator
{
    public bool TryValidate(string input, ObjectPropertyDefinition propertyDefinition, out string normalizedValue, out string errorMessage)
    {
        normalizedValue = input;
        errorMessage = null;

        switch (propertyDefinition.type)
        {
            case ObjectPropertyType.Int:
                if (!int.TryParse(input, out int intValue))
                {
                    errorMessage = "Invalid integer";
                    return false;
                }

                if (propertyDefinition.useMin)
                {
                    intValue = Mathf.Max(intValue, Mathf.RoundToInt(propertyDefinition.min));
                }

                if (propertyDefinition.useMax)
                {
                    intValue = Mathf.Min(intValue, Mathf.RoundToInt(propertyDefinition.max));
                }

                normalizedValue = intValue.ToString();
                return true;

            case ObjectPropertyType.Float:
                if (!float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                {
                    errorMessage = "Invalid float";
                    return false;
                }

                if (propertyDefinition.useMin)
                {
                    floatValue = Mathf.Max(floatValue, propertyDefinition.min);
                }

                if (propertyDefinition.useMax)
                {
                    floatValue = Mathf.Min(floatValue, propertyDefinition.max);
                }

                normalizedValue = floatValue.ToString(CultureInfo.InvariantCulture);
                return true;

            case ObjectPropertyType.Bool:
                if (!bool.TryParse(input, out bool boolValue))
                {
                    errorMessage = "Invalid bool";
                    return false;
                }

                normalizedValue = boolValue.ToString();
                return true;

            case ObjectPropertyType.Enum:
                if (propertyDefinition.enumOptions == null || !propertyDefinition.enumOptions.Contains(input))
                {
                    errorMessage = "Invalid enum value";
                    return false;
                }

                return true;

            case ObjectPropertyType.String:
                return true;
        }

        return false;
    }
}

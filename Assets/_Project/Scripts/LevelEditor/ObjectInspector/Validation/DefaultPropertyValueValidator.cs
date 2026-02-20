using UnityEngine;
using System.Globalization;

public class DefaultPropertyValueValidator : IPropertyValueValidator
{
    public bool TryValidate(string input, ObjectPropertyDefinition objectProperty, out string normalizedValue, out string errorMessage)
    {
        normalizedValue = input;
        errorMessage = null;

        switch (objectProperty.type)
        {
            case ObjectPropertyType.Int:
                if (!int.TryParse(input, out int intValue))
                {
                    errorMessage = "Invalid integer";
                    return false;
                }

                if (objectProperty.useMin)
                {
                    intValue = Mathf.Max(intValue, Mathf.RoundToInt(objectProperty.min));
                }

                if (objectProperty.useMax)
                {
                    intValue = Mathf.Min(intValue, Mathf.RoundToInt(objectProperty.max));
                }

                normalizedValue = intValue.ToString();
                return true;

            case ObjectPropertyType.Float:
                if (!float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                {
                    errorMessage = "Invalid float";
                    return false;
                }

                if (objectProperty.useMin)
                {
                    floatValue = Mathf.Max(floatValue, objectProperty.min);
                }

                if (objectProperty.useMax)
                {
                    floatValue = Mathf.Min(floatValue, objectProperty.max);
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
                if (objectProperty.enumOptions == null || !objectProperty.enumOptions.Contains(input))
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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;

public static class LevelObjectPropertyExtensions
{
    public static string GetString(this IReadOnlyList<LevelObjectProperty> properties, string key, string defaultValue = "")
    {
        LevelObjectProperty property = Find(properties, key);
        if (property == null)
        {
            return defaultValue;
        }

        return property.value;
    }

    public static int GetInt(this IReadOnlyList<LevelObjectProperty> properties, string key, int defaultValue = 0)
    {
        LevelObjectProperty property = Find(properties, key);
        if (property == null)
        {
            return defaultValue;
        }

        if (int.TryParse(property.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        Debug.LogWarning($"[LevelObjectProperty] Cannot parse int from '{property.value}' (key='{key}').");
        return defaultValue;
    }

    public static bool GetBool(this IReadOnlyList<LevelObjectProperty> properties, string key, bool defaultValue = false)
    {
        LevelObjectProperty property = Find(properties, key);
        if (property == null)
        {
            return defaultValue;
        }

        if (bool.TryParse(property.value, out bool result))
        {
            return result;
        }

        Debug.LogWarning($"[LevelObjectProperty] Cannot parse bool from '{property.value}' (key='{key}').");
        return defaultValue;
    }

    public static float GetFloat(this IReadOnlyList<LevelObjectProperty> properties, string key, float defaultValue = 0f)
    {
        LevelObjectProperty property = Find(properties, key);
        if (property == null)
        {
            return defaultValue;
        }

        if (float.TryParse(property.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }

        Debug.LogWarning($"[LevelObjectProperty] Cannot parse float from '{property.value}' (key='{key}').");
        return defaultValue;
    }

    public static TEnum GetEnum<TEnum>(this IReadOnlyList<LevelObjectProperty> properties, string key, TEnum defaultValue) where TEnum : struct, Enum
    {
        LevelObjectProperty property = Find(properties, key);
        if (property == null)
        {
            return defaultValue;
        }

        if (Enum.TryParse<TEnum>(property.value, out var result))
        {
            return result;
        }

        Debug.LogWarning($"[LevelObjectProperty] Cannot parse enum {typeof(TEnum).Name} from '{property.value}'.");
        return defaultValue;
    }

    private static LevelObjectProperty Find(IReadOnlyList<LevelObjectProperty> properties, string key)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            if (properties[i].key == key)
            {
                return properties[i];
            }
        }

        return null;
    }
}

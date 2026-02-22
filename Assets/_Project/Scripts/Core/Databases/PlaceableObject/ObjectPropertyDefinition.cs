using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;
using System.Collections.Generic;

public enum ObjectPropertyType
{
    Int,
    Float,
    Bool,
    String,
    //Color,
    Enum
}

[System.Serializable]
public class ObjectPropertyDefinition
{
    public string key;

    [Header("Presentation")]
    public LocalizedString localizedString;

    [Header("Typing")]
    public ObjectPropertyType type;

    public string defaultValue;

    private bool IsNumericType => type == ObjectPropertyType.Int || type == ObjectPropertyType.Float;

    [ShowIf(nameof(IsNumericType))]
    public bool useMin;

    [ShowIf(nameof(useMin))]
    public float min;

    [ShowIf(nameof(IsNumericType))]
    public bool useMax;

    [ShowIf(nameof(useMax))]
    public float max;

    private bool IsEnumType => type == ObjectPropertyType.Enum;

    [ShowIf(nameof(IsEnumType))]
    public List<string> enumOptions;
}

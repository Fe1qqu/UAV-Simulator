using System;
using UnityEngine;

[Serializable]
public class SettingCondition
{
    public SettingDefinition targetSetting;

    public ConditionOperatorType operatorType;

    [Tooltip("Value used for comparison (int-based index for option settings).")]
    public int compareValue;

    public bool Evaluate(object targetValue)
    {
        int value = Convert.ToInt32(targetValue);

        return operatorType switch
        {
            ConditionOperatorType.Equals => value == compareValue,
            ConditionOperatorType.NotEquals => value != compareValue,
            ConditionOperatorType.Greater => value > compareValue,
            ConditionOperatorType.Less => value < compareValue,
            ConditionOperatorType.GreaterOrEqual => value >= compareValue,
            ConditionOperatorType.LessOrEqual => value <= compareValue,
            _ => false
        };
    }
}

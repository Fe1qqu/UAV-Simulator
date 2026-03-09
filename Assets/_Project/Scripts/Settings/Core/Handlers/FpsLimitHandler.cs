using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings/Handlers/FpsLimit")]
public class FpsLimitHandler : SettingHandlerBase
{
    public override void Apply(SettingInstance setting)
    {
        var rangeSetting = setting.Definition as RangeSettingDefinition;
        if (rangeSetting == null)
        {
            return;
        }

        float value = (float)setting.GetRuntimeValue();

        bool isUnlimited = rangeSetting.hasSpecialMaxValue && Mathf.Approximately(value, rangeSetting.maxValue);

        if (isUnlimited)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = Mathf.RoundToInt(value);
        }
    }
}

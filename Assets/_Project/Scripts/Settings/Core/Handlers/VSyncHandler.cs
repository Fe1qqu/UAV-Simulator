using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings/Handlers/VSync")]
public class VSyncHandler : SettingHandlerBase
{
    public override void Apply(SettingInstance setting)
    {
        bool enabled = (int)setting.GetValue() == 1;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
}

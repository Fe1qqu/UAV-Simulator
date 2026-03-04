using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings/Handlers/Master Volume")]
public class MasterVolumeHandler : SettingHandlerBase
{
    public override void Apply(SettingInstance setting)
    {
        float volume = (float)setting.GetValue();
        AudioListener.volume = Mathf.Clamp01(volume);
    }
}

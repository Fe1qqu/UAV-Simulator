using UnityEngine;

[CreateAssetMenu(menuName = "UI/Tooltip/Default Resolver")]
public class DefaultTooltipSettingsResolver : ScriptableObject, ITooltipSettingsResolver
{
    public TooltipSettings defaultSettings;
    public int Priority => 0;

    public TooltipSettings Resolve(TooltipRequest request) => defaultSettings;
}

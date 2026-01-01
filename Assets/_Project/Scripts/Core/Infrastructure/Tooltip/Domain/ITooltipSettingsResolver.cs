public interface ITooltipSettingsResolver
{
    TooltipSettings Resolve(TooltipRequest request);
    int Priority { get; }
}

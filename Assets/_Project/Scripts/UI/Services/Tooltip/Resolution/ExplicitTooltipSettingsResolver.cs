public class ExplicitTooltipSettingsResolver : ITooltipSettingsResolver
{
    public int Priority => 1000;

    public TooltipSettings Resolve(TooltipRequest request) => request.explicitSettings;
}

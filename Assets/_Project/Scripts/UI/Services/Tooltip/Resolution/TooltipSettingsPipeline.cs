using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class TooltipSettingsPipeline
{
    private readonly List<ITooltipSettingsResolver> resolvers;

    public TooltipSettingsPipeline(IEnumerable<ITooltipSettingsResolver> resolvers)
    {
        this.resolvers = resolvers.OrderByDescending(resolver => resolver.Priority).ToList();
    }

    public TooltipSettings Resolve(TooltipRequest request)
    {
        foreach (ITooltipSettingsResolver resolver in resolvers)
        {
            TooltipSettings settings = resolver.Resolve(request);
            if (settings != null)
            {
                return settings;
            }
        }

        Debug.LogError("[TooltipSettingsPipeline] No settings resolved. This should never happen.");
        return null;
    }
}

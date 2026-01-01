using UnityEngine;

public interface ITooltipSource
{
    TooltipRequest CreateTooltipRequest(GameObject context);
}

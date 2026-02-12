using UnityEngine;
using UnityEngine.Localization;

public struct TooltipRequest
{
    public bool isValid;
    public LocalizedString text;
    public TooltipSettings explicitSettings;  // Settings override
    public GameObject context;                // Source

    public RectTransform fixedAnchor;
    public bool force;

    public static readonly TooltipRequest Invalid = new()
    {
        isValid = false
    };
}

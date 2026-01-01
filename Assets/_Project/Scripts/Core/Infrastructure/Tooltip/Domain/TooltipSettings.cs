using UnityEngine;
using Alchemy.Inspector;

public enum TooltipDisplayMode
{
    FollowPointer,
    FixedAnchor
}

public enum TooltipAnchorSide
{
    Top,      // Above anchor
    Bottom,   // Under anchor
    Left,     // To the left of the anchor
    Right     // To the right of the anchor
}

[CreateAssetMenu(menuName = "UI/Tooltip/Tooltip Settings")]
public class TooltipSettings : ScriptableObject
{
    [Header("Display Mode")]
    public TooltipDisplayMode displayMode = TooltipDisplayMode.FollowPointer;

    private bool IsFollowPointer => displayMode == TooltipDisplayMode.FollowPointer;
    private bool IsFixedAnchor => displayMode == TooltipDisplayMode.FixedAnchor;

    [Header("Pointer Follow")]
    [ShowIf(nameof(IsFollowPointer))]
    public Vector2 tooltipOffset = new Vector2(12f, -8f);

    [Header("Fixed Anchor")]
    [ShowIf(nameof(IsFixedAnchor))]
    public TooltipAnchorSide anchorSide = TooltipAnchorSide.Right;

    [ShowIf(nameof(IsFixedAnchor))]
    [Tooltip("Additional offset to the object's TooltipAnchor.")]
    public Vector2 fixedOffset = Vector3.zero;

    [Header("Timing")]
    public float delay = 0.1f;
    public float fadeInSpeed = 0.1f;
    public float fadeOutSpeed = 0.0f;
}

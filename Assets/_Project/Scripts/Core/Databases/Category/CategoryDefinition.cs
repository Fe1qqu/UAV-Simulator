using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;

[CreateAssetMenu(menuName = "Game Data/Category Definition")]
public class CategoryDefinition : ScriptableObject
{
    [Header("Identity")]
    public string categoryId;

    [Header("Presentation")]
    public LocalizedString localizedString;
    public Sprite icon;

    [Header("Tooltip Override")]
    public bool useTooltipSettingsOverride;

    [ShowIf(nameof(useTooltipSettingsOverride))]
    [Tooltip("Optional override. If set, these tooltip settings will be used instead of those resolved by the TooltipSettingsPipeline.")]
    public TooltipSettings tooltipSettingsOverride;
}

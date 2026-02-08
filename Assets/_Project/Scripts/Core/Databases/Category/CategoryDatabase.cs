using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;
using System.Collections.Generic;

/// <summary>
/// Serializable container for a single category entry.
/// </summary>
[System.Serializable]
public class CategoryData
{
    [Tooltip("Enum value that groups placeable objects.")]
    public PlaceableObjectType type;

    public LocalizedString localizationKey;

    [Tooltip("Icon used for category buttons.")]
    public Sprite icon;

    [Header("Tooltip Override")]
    public bool useTooltipSettingsOverride;

    [ShowIf(nameof(useTooltipSettingsOverride))]
    [Tooltip("Optional override. If set, these tooltip settings will be used instead of those resolved by the TooltipSettingsPipeline.")]
    public TooltipSettings tooltipSettingsOverride;
}

/// <summary>
/// ScriptableObject that holds a list of categories for the level editor.
/// </summary>
[CreateAssetMenu(fileName = "CategoryDatabase", menuName = "Game Data/Category Database")]
public class CategoryDatabase : ScriptableObject
{
    [Tooltip("List of available categories. Each entry defines a category type, name and icon.")]
    public List<CategoryData> categories = new();
}

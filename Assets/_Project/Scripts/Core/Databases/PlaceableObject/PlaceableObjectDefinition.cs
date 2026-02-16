using UnityEngine;
using UnityEngine.Localization;
using Alchemy.Inspector;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPropertyDefinition
{
    public string key;
    public string defaultValue;
}
public enum PreviewMaterialMode
{
    UseDefault,   // use the general previewMaterial
    Override,     // use previewMaterialOverride
    None          // do not use the material at all
}

[CreateAssetMenu(menuName = "Game Data/Placeable Object Definition")]
public class PlaceableObjectDefinition : ScriptableObject
{
    [Header("Identity")]
    public string objectId;

    [Header("Presentation")]
    public LocalizedString localizedString;

    [Tooltip("Icon shown on the placeable object's button.")]
    public Sprite icon;

    [Header("Runtime")]
    [Tooltip("Prefab instantiated when the object is placed in the scene.")]
    public GameObject prefab;

    [Header("Category")]
    public CategoryDefinition category;

    [Tooltip("Predefined properties for this object. May be empty if the object has no properties.")]
    public List<ObjectPropertyDefinition> propertyDefinitions = new();

    [Header("Preview")]
    public PreviewMaterialMode previewMaterialMode = PreviewMaterialMode.UseDefault;

    private bool IsPreviewOverride => previewMaterialMode == PreviewMaterialMode.Override;

    [ShowIf(nameof(IsPreviewOverride))]
    public Material previewMaterialOverride;

    [Header("Tooltip Override")]
    public bool useTooltipSettingsOverride;

    [ShowIf(nameof(useTooltipSettingsOverride))]
    [Tooltip("Optional override. If set, these tooltip settings will be used instead of those resolved by the TooltipSettingsPipeline.")]
    public TooltipSettings tooltipSettingsOverride;
}

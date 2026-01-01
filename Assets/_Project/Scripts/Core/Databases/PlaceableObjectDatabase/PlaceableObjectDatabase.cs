using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

/// <summary>
/// Types used to classify placeable objects in the editor.
/// </summary>
public enum PlaceableObjectType
{
    Generic,
    Tree,
    Building,
    Light,
    Gameplay
}

public enum PreviewMaterialMode
{
    UseDefault,   // use the general previewMaterial
    Override,     // use previewMaterialOverride
    None          // do not use the material at all
}

/// <summary>
/// Serializable container for a single placeable object definition.
/// </summary>
[System.Serializable]
public class PlaceableObjectData : ITooltipSource
{
    [Header("Identity")]
    [Tooltip("Stable unique identifier used for saving/loading.")]
    public string objectId;

    [Header("Presentation")]
    public LocalizedString localizationKey;
    
    [Tooltip("Icon shown on the placeable object's button.")]
    public Sprite icon;

    [Header("Runtime")]
    [Tooltip("Prefab instantiated when the object is placed in the scene.")]
    public GameObject prefab;

    [Tooltip("Category/type used for filtering in the editor UI.")]
    public PlaceableObjectType type = PlaceableObjectType.Generic;

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

    public TooltipRequest CreateTooltipRequest(GameObject context)
    {
        return new TooltipRequest
        {
            text = localizationKey,
            explicitSettings = tooltipSettingsOverride,
            context = context
        };
    }
}

/// <summary>
/// ScriptableObject that stores all placeable objects available to the level editor.
/// </summary>
[CreateAssetMenu(fileName = "PlaceableObjectDatabase", menuName = "Game Data/Placeable Object Database")]
public class PlaceableObjectDatabase : ScriptableObject
{
    [Tooltip("All placeable object entries. Used to spawn buttons and instantiate prefabs.")]
    public List<PlaceableObjectData> objects = new List<PlaceableObjectData>();

    /// <summary>
    /// Returns all objects filtered by type.
    /// </summary>
    public List<PlaceableObjectData> GetByType(PlaceableObjectType type)
    {
        if (objects == null)
        {
            return new List<PlaceableObjectData>();
        }

        return objects.FindAll(obj => obj != null && obj.type == type);
    }

    public PlaceableObjectData GetById(string id)
    {
        return objects.Find(obj => obj.objectId == id);
    }
}

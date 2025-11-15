using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Types used to classify placeable objects in the editor.
/// </summary>
public enum PlaceableObjectType
{
    Generic,
    Tree,
    Building,
    Light,
    Prop,
    Decoration
}

/// <summary>
/// Serializable container for a single placeable object definition.
/// </summary>
[System.Serializable]
public class PlaceableObjectData
{
    [Tooltip("Unique display name used in UI lists.")]
    public string name;

    [Tooltip("Icon shown on the placeable object's button.")]
    public Sprite icon;

    [Tooltip("Prefab instantiated when the object is placed in the scene.")]
    public GameObject prefab;

    [Tooltip("Category/type used for filtering in the editor UI.")]
    public PlaceableObjectType type = PlaceableObjectType.Generic;
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
}

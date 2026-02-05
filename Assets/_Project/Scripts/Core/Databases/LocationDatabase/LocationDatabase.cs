using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

/// <summary>
/// Serializable container for a level/location entry.
/// </summary>
[System.Serializable]
public class LocationData
{
    [Header("Identity")]
    [Tooltip("Stable unique identifier used for saving/loading.")]
    public string locationId;

    [Header("Presentation")]
    public LocalizedString localizationKey;

    [Tooltip("Preview thumbnail shown in the UI.")]
    public Sprite icon;

    [Header("Runtime")]
    [Tooltip("Prefab representing the location/scene root to instantiate.")]
    public GameObject prefab;
}

/// <summary>
/// ScriptableObject that stores available locations for the level editor.
/// </summary>
[CreateAssetMenu(fileName = "LocationDatabase", menuName = "Game Data/Location Database")]
public class LocationDatabase : ScriptableObject
{
    [Tooltip("List of locations available to pick from in the level editor.")]
    public List<LocationData> locations = new();
}

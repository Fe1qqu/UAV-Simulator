using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Game Data/Location Definition")]
public class LocationDefinition : ScriptableObject
{
    [Header("Identity")]
    public string locationId;

    [Header("Presentation")]
    public LocalizedString localizedString;

    [Header("Runtime")]
    public GameObject prefab;
}

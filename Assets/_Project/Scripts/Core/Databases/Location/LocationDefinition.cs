using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Game Data/Location Definition")]
public class LocationDefinition : ScriptableObject
{
    [Header("Identity")]
    public string locationId;

    [Header("Presentation")]
    public LocalizedString localizationKey;
    public Sprite icon;

    [Header("Runtime")]
    public GameObject prefab;
}

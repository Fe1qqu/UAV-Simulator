using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Location Database")]
public class LocationDatabase : ScriptableObject
{
    public List<LocationDefinition> locations = new();

    public LocationDefinition GetById(string id)
    {
        return locations.Find(location => location.locationId == id);
    }
}

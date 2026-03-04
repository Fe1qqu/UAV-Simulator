using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Placeable Objects Database")]
public class PlaceableObjectsDatabase : ScriptableObject
{
    public List<PlaceableObjectDefinition> objects = new();

    public List<PlaceableObjectDefinition> GetByCategory(CategoryDefinition category)
    {
        return objects.FindAll(obj => obj.category == category);
    }

    public PlaceableObjectDefinition GetById(string id)
    {
        return objects.Find(obj => obj.objectId == id);
    }
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Placeable Object Database")]
public class PlaceableObjectDatabase : ScriptableObject
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

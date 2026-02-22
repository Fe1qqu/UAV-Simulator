using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Category Database")]
public class CategoryDatabase : ScriptableObject
{
    public List<CategoryDefinition> categories = new();

    public CategoryDefinition GetById(string id)
    {
        return categories.Find(category => category.categoryId == id);
    }
}

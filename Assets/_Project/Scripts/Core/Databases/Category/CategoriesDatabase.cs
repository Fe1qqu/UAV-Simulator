using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Categories Database")]
public class CategoriesDatabase : ScriptableObject
{
    public List<CategoryDefinition> categories = new();

    public CategoryDefinition GetById(string id)
    {
        return categories.Find(category => category.categoryId == id);
    }
}

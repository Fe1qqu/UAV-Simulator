using UnityEngine;
using Alchemy.Inspector;
using System.Collections.Generic;

public enum ScenarioCategoryAccessMode
{
    All,              // whole category is available
    ListedOnly,       // only objects from the allowed list
    AllExceptListed   // all objects except those on the excluded list
}

[System.Serializable]
public class ScenarioCategoryRule
{
    public CategoryDefinition category;

    public ScenarioCategoryAccessMode accessMode;

    private bool UseObjectList => accessMode != ScenarioCategoryAccessMode.All;

    [ShowIf(nameof(UseObjectList))]
    [Tooltip("For ListedOnly it is allowed objects, for AllExceptListed it is excluded objects.")]
    public List<string> objectIds = new();
}

using Alchemy.Inspector;
using System.Collections.Generic;

public enum ScenarioCategoryAccessMode
{
    AllObjects,        // whole category is available
    OnlyListedObjects  // only listed objects is available
}

[System.Serializable]
public class ScenarioCategoryRule
{
    public PlaceableObjectType category;
    public ScenarioCategoryAccessMode accessMode;

    private bool UseOnlyListedObjects => accessMode == ScenarioCategoryAccessMode.OnlyListedObjects;

    [ShowIf(nameof(UseOnlyListedObjects))]
    public List<string> allowedObjectIds = new();
}

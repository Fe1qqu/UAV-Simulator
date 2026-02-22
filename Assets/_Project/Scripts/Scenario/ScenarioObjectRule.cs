using UnityEngine;

[System.Serializable]
public class ScenarioObjectRule
{
    public PlaceableObjectDefinition placeableObject;

    [Min(0)]
    public int minCount = 0;

    [Tooltip("-1 = unlimited")]
    public int maxCount = -1;
}

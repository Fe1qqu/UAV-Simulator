using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public string locationId;
    public string scenarioId;
    public List<LevelObjectData> objects = new();
}

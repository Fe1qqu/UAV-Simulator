using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    //public int version = 1;
    public string levelName;
    public string locationId;
    public string scenarioId;
    public List<LevelObjectData> objects = new();
}

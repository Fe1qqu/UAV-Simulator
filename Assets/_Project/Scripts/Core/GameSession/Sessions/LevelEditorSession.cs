public class LevelEditorSession
{
    public string LevelName { get; private set; }
    public string LocationId { get; private set; }
    public string ScenarioId { get; private set; }
    public string LevelFilePath { get; private set; }

    public void Setup(string levelName, string locationId, string scenarioId)
    {
        LevelName = levelName;
        LocationId = locationId;
        ScenarioId = scenarioId;
    }

    public void SetLevelFilePath(string path)
    {
        LevelFilePath = path;
    }

    public void Clear()
    {
        LevelName = null;
        LocationId = null;
        ScenarioId = null;
        LevelFilePath = null;
    }
}

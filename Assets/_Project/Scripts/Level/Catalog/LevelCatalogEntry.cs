public readonly struct LevelCatalogEntry
{
    public readonly string FilePath;
    public readonly LevelData LevelData;
    public readonly string ScenarioDisplayName;
    public readonly string LocationDisplayName;

    public LevelCatalogEntry(string filePath, LevelData levelData, string scenarioDisplayName, string locationDisplayName)
    {
        FilePath = filePath;
        LevelData = levelData;
        ScenarioDisplayName = scenarioDisplayName;
        LocationDisplayName = locationDisplayName;
    }
}

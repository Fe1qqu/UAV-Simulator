public readonly struct LevelCatalogEntry
{
    public readonly string FilePath;
    public readonly LevelData LevelData;
    public readonly string ScenarioDisplayName;

    public LevelCatalogEntry(string filePath, LevelData levelData, string scenarioDisplayName)
    {
        FilePath = filePath;
        LevelData = levelData;
        ScenarioDisplayName = scenarioDisplayName;
    }
}

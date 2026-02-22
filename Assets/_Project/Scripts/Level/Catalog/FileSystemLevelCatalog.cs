using UnityEngine;
using System.Collections.Generic;
using System.IO;

public sealed class FileSystemLevelCatalog : ILevelCatalog
{
    private readonly LevelFileManager levelFileManager;
    private readonly ScenarioDatabase scenarioDatabase;

    public FileSystemLevelCatalog(LevelFileManager levelFileManager, ScenarioDatabase scenarioDatabase)
    {
        this.levelFileManager = levelFileManager;
        this.scenarioDatabase = scenarioDatabase;
    }

    public IReadOnlyList<LevelCatalogEntry> GetAll()
    {
        List<LevelCatalogEntry> result = new();

        string directory = Path.Combine(Application.persistentDataPath, "levels");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        foreach (string filePath in Directory.GetFiles(directory, "*.json"))
        {
            LevelData levelData = levelFileManager.LoadByPath(filePath);
            if (levelData == null)
            {
                continue;
            }

            string scenarioDisplayName = ResolveScenarioName(levelData.scenarioId);

            result.Add(new LevelCatalogEntry(filePath, levelData, scenarioDisplayName));
        }

        return result;
    }

    private string ResolveScenarioName(string scenarioId)
    {
        if (string.IsNullOrEmpty(scenarioId))
        {
            return "—";
        }

        ScenarioDefinition scenario = scenarioDatabase.GetById(scenarioId);
        if (scenario == null)
        {
            return "—";
        }

        return scenario.localizedString.GetLocalizedString();
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public sealed class FileSystemLevelCatalog : ILevelCatalog
{
    private readonly LevelFileManager levelFileManager;

    public FileSystemLevelCatalog(LevelFileManager levelFileManager)
    {
        this.levelFileManager = levelFileManager;
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
            string locationDisplayName = ResolveLocationName(levelData.locationId);

            result.Add(new LevelCatalogEntry(filePath, levelData, scenarioDisplayName, locationDisplayName));
        }

        return result;
    }

    private string ResolveScenarioName(string scenarioId)
    {
        if (string.IsNullOrEmpty(scenarioId))
        {
            return "—";
        }

        ScenarioDefinition scenario = GameDataManager.Instance.Scenarios.GetById(scenarioId);
        if (scenario == null)
        {
            return "—";
        }

        return scenario.localizedString.GetLocalizedString();
    }

    private string ResolveLocationName(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
        {
            return "—";
        }

        LocationDefinition location = GameDataManager.Instance.Locations.GetById(locationId);
        if (location == null)
        {
            return "—";
        }

        return location.localizedString.GetLocalizedString();
    }
}

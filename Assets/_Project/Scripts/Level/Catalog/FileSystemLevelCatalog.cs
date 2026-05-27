using UnityEngine;
using System.Collections.Generic;
using System.IO;

public sealed class FileSystemLevelCatalog : ILevelCatalog
{
    private readonly LevelFileManager levelFileManager;

    private readonly ScenariosDatabase scenariosDatabase;
    private readonly LocationsDatabase locationsDatabase;

    public FileSystemLevelCatalog(LevelFileManager levelFileManager, ScenariosDatabase scenariosDatabase, LocationsDatabase locationsDatabase)
    {
        this.levelFileManager = levelFileManager;
        this.scenariosDatabase = scenariosDatabase;
        this.locationsDatabase = locationsDatabase;
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

        ScenarioDefinition scenario = scenariosDatabase.GetById(scenarioId);
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

        LocationDefinition location = locationsDatabase.GetById(locationId);
        if (location == null)
        {
            return "—";
        }

        return location.localizedString.GetLocalizedString();
    }
}

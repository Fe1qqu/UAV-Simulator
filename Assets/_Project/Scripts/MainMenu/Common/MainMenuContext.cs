using UnityEngine;

public class MainMenuContext : MonoBehaviour
{
    [Header("Persistence")]
    [SerializeField] private LevelFileManager levelFileManager;

    [Header("Databases")]
    [SerializeField] private LocationDatabase locationDatabase;
    [SerializeField] private ScenarioDatabase scenarioDatabase;

    public LocationDatabase LocationDatabase => locationDatabase;
    public ScenarioDatabase ScenarioDatabase => scenarioDatabase;

    private ILevelCatalog levelCatalog;
    public ILevelCatalog LevelCatalog => levelCatalog;

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[MainMenuContext] LevelFileManager is not assigned.");
            return;
        }

        if (locationDatabase == null || locationDatabase.locations.Count == 0)
        {
            Debug.LogError("[MainMenuContext] LocationDatabase is missing or empty.");
            return;
        }

        if (scenarioDatabase == null || scenarioDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[MainMenuContext] ScenarioDatabase is missing or empty.");
            return;
        }

        levelCatalog = new FileSystemLevelCatalog(levelFileManager, scenarioDatabase);
    }
}

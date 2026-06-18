using UnityEngine;

public class MainMenuContext : MonoBehaviour
{
    [Header("Persistence")]
    [SerializeField] private LevelFileManager levelFileManager;

    [Header("Databases")]
    [SerializeField] private LocationsDatabase locationsDatabase;
    [SerializeField] private ScenariosDatabase scenariosDatabase;

    public LocationsDatabase LocationsDatabase => locationsDatabase;
    public ScenariosDatabase ScenariosDatabase => scenariosDatabase;

    public LevelFileManager LevelFileManager => levelFileManager;


    private ILevelCatalog levelCatalog;
    public ILevelCatalog LevelCatalog => levelCatalog;

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[MainMenuContext] LevelFileManager is not assigned.");
            return;
        }

        if (locationsDatabase == null || locationsDatabase.locations.Count == 0)
        {
            Debug.LogError("[MainMenuContext] LocationsDatabase is missing or empty.");
            return;
        }

        if (scenariosDatabase == null || scenariosDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[MainMenuContext] ScenariosDatabase is missing or empty.");
            return;
        }

        levelCatalog = new FileSystemLevelCatalog(levelFileManager, scenariosDatabase, locationsDatabase);
    }
}

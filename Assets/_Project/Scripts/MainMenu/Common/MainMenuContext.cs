using UnityEngine;

public class MainMenuContext : MonoBehaviour
{
    [Header("Persistence")]
    [SerializeField] private LevelFileManager levelFileManager;

    [Header("Databases")]
    [SerializeField] private ScenarioDatabase scenarioDatabase;

    private ILevelCatalog levelCatalog;
    public ILevelCatalog LevelCatalog => levelCatalog;

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[MainMenuContext] LevelFileManager is not assigned.");
            return;
        }

        if (scenarioDatabase == null)
        {
            Debug.LogError("[MainMenuContext] ScenarioDatabase is not assigned.");
            return;
        }

        levelCatalog = new FileSystemLevelCatalog(levelFileManager, scenarioDatabase);
    }
}

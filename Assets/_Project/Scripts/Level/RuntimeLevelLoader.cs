using UnityEngine;

public class RuntimeLevelLoader : MonoBehaviour
{
    [SerializeField] private LevelSaveManager levelSaveManager;
    [SerializeField] private LevelLoader levelLoader;

    [Header("Runtime")]
    [SerializeField] private DroneControllerBase dronePrefab;

    private DroneControllerBase spawnedDrone;

    private void Awake()
    {
        if (levelSaveManager == null)
        {
            Debug.LogError("[RuntimeLevelLoader] LevelSaveManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[RuntimeLevelLoader] LevelLoader is not assigned.");
        }

        if (dronePrefab == null)
        {
            Debug.LogError("[RuntimeLevelLoader] DronePrefab is not assigned.");
        }
    }

    private void Start()
    {
        PlaySession playSession = GameSettings.Instance.CurrentPlaySession;

        LevelData data = levelSaveManager.LoadByPath(playSession.LevelFilePath);
        levelLoader.Load(data);

        //ValidateLevel();
        SpawnDrone();
    }

    //private void ValidateLevel()
    //{
    //    var required = FindObjectsOfType<MonoBehaviour>()
    //        .OfType<IRequiredLevelObject>();

    //    if (!required.Any())
    //    {
    //        Debug.LogError("[RuntimeLevelLoader] No required level objects found.");
    //    }
    //}

    private void SpawnDrone()
    {
        DroneSpawnPoint spawnPoint = FindFirstObjectByType<DroneSpawnPoint>();
        if (spawnPoint == null)
        {
            Debug.LogError("[RuntimeLevelLoader] DroneSpawnPoint not found.");
            return;
        }

        if (dronePrefab == null)
        {
            Debug.LogError("[RuntimeLevelLoader] DronePrefab is not assigned.");
            return;
        }

        spawnedDrone = Instantiate(dronePrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
    }
}

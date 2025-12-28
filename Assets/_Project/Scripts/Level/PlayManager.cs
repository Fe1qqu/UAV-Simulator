using UnityEngine;

public class PlayManager : MonoBehaviour
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
            Debug.LogError("[PlayManager] LevelSaveManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[PlayManager] LevelLoader is not assigned.");
        }

        if (dronePrefab == null)
        {
            Debug.LogError("[PlayManager] DronePrefab is not assigned.");
        }
    }

    private void Start()
    {
        LoadLevel();
        //ValidateLevel();
        SpawnDrone();
    }

    private void LoadLevel()
    {
        PlaySession playSession = GameSettings.Instance.CurrentPlaySession;
        LevelData data = levelSaveManager.LoadByPath(playSession.LevelFilePath);
        levelLoader.Load(data);
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
            Debug.LogError("[PlayManager] DroneSpawnPoint not found.");
            return;
        }

        if (dronePrefab == null)
        {
            Debug.LogError("[PlayManager] DronePrefab is not assigned.");
            return;
        }

        spawnedDrone = Instantiate(dronePrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
    }
}

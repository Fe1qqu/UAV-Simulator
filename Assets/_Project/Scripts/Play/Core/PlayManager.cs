using UnityEngine;

public class PlayManager : MonoBehaviour, IBackHandler
{
    [SerializeField] private PlayInput playInput;
    [SerializeField] private LevelSaveManager levelSaveManager;
    [SerializeField] private LevelLoader levelLoader;

    [Header("UI References")]
    [SerializeField] private PlayPauseMenu pauseMenu;

    [Header("Runtime")]
    [SerializeField] private DroneControllerBase dronePrefab;

    private DroneControllerBase spawnedDrone;

    private void Awake()
    {
        if (playInput == null)
        {
            Debug.LogError("[PlayManager] PlayInput is not assigned.");
        }

        if (levelSaveManager == null)
        {
            Debug.LogError("[PlayManager] LevelSaveManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[PlayManager] LevelLoader is not assigned.");
        }

        if (pauseMenu == null)
        {
            Debug.LogError("[PlayManager] PauseMenu is not assigned.");
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

    private void OnEnable()
    {
        playInput.RestartRequested += OnRestartRequested;
    }

    private void OnDisable()
    {
        playInput.RestartRequested -= OnRestartRequested;
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

    private void OnRestartRequested()
    {
        if (pauseMenu.IsOpen)
        {
            return;
        }

        RestartLevel();
    }

    public void RestartLevel()
    {
        ResetDrone();

        Debug.Log("[PlayManager] Level restarted.");
    }

    public void ResetDrone()
    {
        if (spawnedDrone == null)
        {
            Debug.LogWarning("[PlayManager] Cannot reset drone: drone is null.");
            return;
        }

        DroneSpawnPoint spawnPoint = FindFirstObjectByType<DroneSpawnPoint>();
        if (spawnPoint == null)
        {
            Debug.LogError("[PlayManager] DroneSpawnPoint not found.");
            return;
        }

        // Resetting the drone's internal state
        spawnedDrone.ResetState();

        // Return drone to spawn point
        spawnedDrone.transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
    }

    public bool OnBack()
    {
        Debug.Log("[PlayManager] OnBack.");
        pauseMenu.Open();
        return true;
    }
}

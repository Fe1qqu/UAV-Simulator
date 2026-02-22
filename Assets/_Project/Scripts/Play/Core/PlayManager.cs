using UnityEngine;

public class PlayManager : MonoBehaviour, IBackHandler
{
    [Header("Core")]
    [SerializeField] private PlayInput playInput;
    [SerializeField] private LevelFileManager levelFileManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;
    [SerializeField] private ScenarioDatabase scenarioDatabase;

    [Header("UI")]
    [SerializeField] private PlayPauseMenu pauseMenu;

    [Header("Runtime")]
    [SerializeField] private DroneControllerBase dronePrefab;

    private DroneControllerBase spawnedDrone;
    private IScenarioRuntime scenarioRuntime;
    private LevelData loadedLevelData;

    private void Awake()
    {
        if (playInput == null)
        {
            Debug.LogError("[PlayManager] PlayInput is not assigned.");
        }

        if (levelFileManager == null)
        {
            Debug.LogError("[PlayManager] LevelFileManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[PlayManager] LevelLoader is not assigned.");
        }

        if (levelObjectRegistry == null)
        {
            Debug.LogError("[PlayManager] LevelObjectRegistry is not assigned.");
        }

        if (scenarioDatabase == null || scenarioDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[PlayManager] ScenarioDatabase is missing or empty.");
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

    private void OnEnable()
    {
        playInput.RestartRequested += OnRestartRequested;
    }

    private void OnDisable()
    {
        playInput.RestartRequested -= OnRestartRequested;
    }

    private void Start()
    {
        LoadLevel();
        SpawnDrone();
        StartScenario();
    }

    private void Update()
    {
        scenarioRuntime?.TickScenario();
    }

    private void LoadLevel()
    {
        PlaySession playSession = GameSettings.Instance.CurrentPlaySession;

        loadedLevelData = levelFileManager.LoadByPath(playSession.LevelFilePath);
        if (loadedLevelData == null)
        {
            Debug.LogError("[PlayManager] Failed to load LevelData.");
            return;
        }

        levelLoader.Load(loadedLevelData);
    }

    private void SpawnDrone()
    {
        DroneSpawnPoint droneSpawnPoint = levelObjectRegistry.FindFirstAlive<DroneSpawnPoint>();
        if (droneSpawnPoint == null)
        {
            Debug.LogError("[PlayManager] DroneSpawnPoint not found.");
            return;
        }

        spawnedDrone = Instantiate(dronePrefab, droneSpawnPoint.transform.position, droneSpawnPoint.transform.rotation);
    }

    private void StartScenario()
    {
        if (string.IsNullOrEmpty(loadedLevelData.scenarioId))
        {
            Debug.LogError("[PlayManager] LevelData.scenarioId is empty.");
            return;
        }

        ScenarioDefinition scenario = scenarioDatabase.GetById(loadedLevelData.scenarioId);
        if (scenario == null)
        {
            Debug.LogError($"[PlayManager] Scenario '{loadedLevelData.scenarioId}' not found in database.");
            return;
        }

        if (scenario.runtime == null)
        {
            Debug.LogError($"[PlayManager] No runtime bound for scenario '{scenario.scenarioId}'.");
            return;
        }
        scenarioRuntime = Instantiate(scenario.runtime);

        scenarioRuntime.Initialize(levelObjectRegistry, spawnedDrone);
        scenarioRuntime.ScenarioCompleted += OnScenarioCompleted;
        scenarioRuntime.StartScenario();
    }

    private void OnDestroy()
    {
        if (scenarioRuntime != null)
        {
            scenarioRuntime.ScenarioCompleted -= OnScenarioCompleted;
            scenarioRuntime.DisposeScenario();
            scenarioRuntime = null;
        }
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

        scenarioRuntime?.ResetScenario();

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

    private void OnScenarioCompleted(IScenarioRuntime _)
    {
        Debug.Log("[PlayManager] Scenario completed!");
    }

    public bool OnBack()
    {
        //Debug.Log("[PlayManager] OnBack.");
        pauseMenu.Open();
        return true;
    }
}

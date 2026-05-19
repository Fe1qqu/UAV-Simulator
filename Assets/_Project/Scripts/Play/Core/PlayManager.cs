using UnityEngine;
using System.Threading.Tasks;

public class PlayManager : MonoBehaviour, IBackHandler, ISceneInitializable
{
    [Header("Core")]
    [SerializeField] private PlayInput playInput;
    [SerializeField] private LevelFileManager levelFileManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;
    [SerializeField] private ScenariosDatabase scenariosDatabase;

    [Header("UI")]
    [SerializeField] private PlayPauseMenu pauseMenu;
    //[SerializeField] private PlayResultMenu resultMenu;

    [Header("Runtime")]
    [SerializeField] private DroneControllerBase dronePrefab;
    [SerializeField] private Transform droneRoot;

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

        if (scenariosDatabase == null || scenariosDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[PlayManager] ScenariosDatabase is missing or empty.");
        }

        if (pauseMenu == null)
        {
            Debug.LogError("[PlayManager] PauseMenu is not assigned.");
        }

        if (dronePrefab == null)
        {
            Debug.LogError("[PlayManager] DronePrefab is not assigned.");
        }
        
        if (droneRoot == null)
        {
            Debug.LogError("[PlayManager] DroneRoot is not assigned.");
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

    public async Task InitializeAsync()
    {
        await InitializePlayAsync();
    }

    private async Task InitializePlayAsync()
    {
        LoadLevel();
        SpawnDrone();
        StartScenario();

        await Task.CompletedTask;
    }

    private void Update()
    {
        scenarioRuntime?.TickScenario();
    }

    private void FixedUpdate()
    {
        scenarioRuntime?.FixedTickScenario();
    }

    private void LoadLevel()
    {
        PlaySession playSession = GameSession.Instance.Play;

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
            Debug.LogError("[PlayManager] Alive DroneSpawnPoint not found.");
            return;
        }

        spawnedDrone = Instantiate(dronePrefab, droneSpawnPoint.transform.position, droneSpawnPoint.transform.rotation, droneRoot);
    }

    private void StartScenario()
    {
        if (string.IsNullOrEmpty(loadedLevelData.scenarioId))
        {
            Debug.LogError("[PlayManager] LevelData.scenarioId is empty.");
            return;
        }

        ScenarioDefinition scenario = scenariosDatabase.GetById(loadedLevelData.scenarioId);
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
        scenarioRuntime.ScenarioFailed += OnScenarioFailed;
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

        DroneSpawnPoint droneSpawnPoint = levelObjectRegistry.FindFirstAlive<DroneSpawnPoint>();
        if (droneSpawnPoint == null)
        {
            Debug.LogError("[PlayManager] Alive DroneSpawnPoint not found.");
            return;
        }

        // Resetting the drone's internal state
        spawnedDrone.ResetState(droneSpawnPoint.transform.position, droneSpawnPoint.transform.rotation);
    }

    private void OnScenarioCompleted(IScenarioRuntime _)
    {
        Debug.Log("[PlayManager] Scenario completed!");
    }

    private void OnScenarioFailed(IScenarioRuntime _)
    {
        Debug.Log("[PlayManager] Scenario failed!");
    }

    public bool OnBack()
    {
        //Debug.Log("[PlayManager] OnBack.");
        pauseMenu.Open();
        return true;
    }
}

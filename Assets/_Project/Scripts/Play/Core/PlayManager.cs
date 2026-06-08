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
    [SerializeField] private PlayResultMenu resultMenu;

    [Header("Runtime")]
    [SerializeField] private UAVControllerBase uavPrefab;
    [SerializeField] private Transform uavRoot;
    [SerializeField] private CheckpointPath checkpointPath;

    private UAVDeathHandler uavDeathHandler;
    private UAVControllerBase spawnedUAV;
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

        if (resultMenu == null)
        {
            Debug.LogError("[PlayManager] ResultMenu is not assigned.");
        }

        if (uavPrefab == null)
        {
            Debug.LogError("[PlayManager] UAVPrefab is not assigned.");
        }
        
        if (uavRoot == null)
        {
            Debug.LogError("[PlayManager] UAVRoot is not assigned.");
        }

        if (checkpointPath == null)
        {
            Debug.LogError("[PlayManager] СheckpointPath is not assigned.");
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

        ConfigureScenarioVisuals();

        SpawnUAV();
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

    private void ConfigureScenarioVisuals()
    {
        ScenarioDefinition scenario = scenariosDatabase.GetById(loadedLevelData.scenarioId);
        bool usesCheckpointPath = scenario != null && scenario.usesCheckpointPath;
        checkpointPath.SetScenarioActive(usesCheckpointPath);
    }

    private void SpawnUAV()
    {
        UAVSpawnPoint uavSpawnPoint = levelObjectRegistry.FindFirstAlive<UAVSpawnPoint>();
        if (uavSpawnPoint == null)
        {
            Debug.LogError("[PlayManager] Alive UAVSpawnPoint not found.");
            return;
        }

        spawnedUAV = Instantiate(uavPrefab, uavSpawnPoint.transform.position, uavSpawnPoint.transform.rotation, uavRoot);
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

        scenarioRuntime.Initialize(levelObjectRegistry, spawnedUAV);
        scenarioRuntime.GameplayConcluded += OnGameplayConcluded;
        scenarioRuntime.ScenarioCompleted += OnScenarioCompleted;
        scenarioRuntime.ScenarioFailed += OnScenarioFailed;
        scenarioRuntime.StartScenario();

        //InputModeController.Instance.SetMode(InputMode.Play);
    }

    private void OnDestroy()
    {
        if (scenarioRuntime != null)
        {
            scenarioRuntime.GameplayConcluded -= OnGameplayConcluded;
            scenarioRuntime.ScenarioCompleted -= OnScenarioCompleted;
            scenarioRuntime.ScenarioFailed -= OnScenarioFailed;
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
        resultMenu.Close();
        ResetUAV();
        scenarioRuntime?.ResetScenario();
        InputModeController.Instance.SetMode(InputMode.Play);

        Debug.Log("[PlayManager] Level restarted.");
    }

    public void ResetUAV()
    {
        if (spawnedUAV == null)
        {
            Debug.LogWarning("[PlayManager] Cannot reset uav: uav is null.");
            return;
        }

        UAVSpawnPoint uavSpawnPoint = levelObjectRegistry.FindFirstAlive<UAVSpawnPoint>();
        if (uavSpawnPoint == null)
        {
            Debug.LogError("[PlayManager] Alive UAVSpawnPoint not found.");
            return;
        }

        // Resetting the uav's internal state
        spawnedUAV.ResetState(uavSpawnPoint.transform.position, uavSpawnPoint.transform.rotation);
    }

    private void OnGameplayConcluded(IScenarioRuntime _)
    {
        InputModeController.Instance.SetMode(InputMode.Disabled);
        Debug.Log("[PlayManager] Gameplay concluded. Disabling input.");
    }

    private void OnScenarioCompleted(IScenarioRuntime _)
    {
        resultMenu.OpenCompleted();
        InputModeController.Instance.SetMode(InputMode.UI);
        Debug.Log("[PlayManager] Scenario completed!");
    }

    private void OnScenarioFailed(IScenarioRuntime _)
    {
        resultMenu.OpenFailed();
        InputModeController.Instance.SetMode(InputMode.UI);
        Debug.Log("[PlayManager] Scenario failed!");
    }

    public bool OnBack()
    {
        if (scenarioRuntime.IsGameplayConcluded)
        {
            return true;
        }

        //Debug.Log("[PlayManager] OnBack.");
        pauseMenu.Open();
        return true;
    }
}

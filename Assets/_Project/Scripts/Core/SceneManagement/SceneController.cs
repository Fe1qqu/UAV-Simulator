using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private LoadingOverlay loadingOverlay;

    private readonly Dictionary<SceneSlot, string> loadedSceneBySlot = new();

    private bool isBusy = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[SceneController] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        if (loadingOverlay == null)
        {
            Debug.LogError("[SceneController] LoadingOverlay is not assigned.");
        }
    }

    public async Task WaitUntilFree()
    {
        while (isBusy)
        {
            await Task.Yield();
        }
    }

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    private Task ExecutePlan(SceneTransitionPlan sceneTransitionPlan)
    {
        if (isBusy)
        {
            Debug.LogWarning("[SceneController] Scene change already in progress.");
            return Task.CompletedTask;
        }

        isBusy = true;

        InputModeController.Instance.SetMode(InputMode.Loading);

        return ChangeSceneAsync(sceneTransitionPlan);
    }

    private async Task ChangeSceneAsync(SceneTransitionPlan sceneTransitionPlan)
    {
        if (sceneTransitionPlan.UseOverlay)
        {
            await loadingOverlay.FadeInAsync();
        }

        foreach (SceneSlot sceneSlot in sceneTransitionPlan.ScenesToUnload)
        {
            await UnloadSceneAsync(sceneSlot);
        }

        if (sceneTransitionPlan.ClearUnusedAssets)
        {
            await CleanupUnusedAssetsAsync();
        }

        foreach (var keyValuePair in sceneTransitionPlan.ScenesToLoad)
        {
            if (loadedSceneBySlot.ContainsKey(keyValuePair.Key))
            {
                await UnloadSceneAsync(keyValuePair.Key);
            }

            await LoadSceneAdditiveAsync(keyValuePair.Key, keyValuePair.Value, sceneTransitionPlan.ActiveSceneName == keyValuePair.Value);
        }

        if (sceneTransitionPlan.UseOverlay)
        {
            await loadingOverlay.FadeOutAsync();
        }

        if (sceneTransitionPlan.TargetInputMode.HasValue)
        {
            InputModeController.Instance.SetMode(sceneTransitionPlan.TargetInputMode.Value);
        }

        isBusy = false;
    }

    private async Task LoadSceneAdditiveAsync(SceneSlot sceneSlot, string sceneName, bool setActive)
    {
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadAsyncOperation == null)
        {
            Debug.LogError($"[SceneController] Failed to load scene {sceneName}.");
            return;
        }

        loadAsyncOperation.allowSceneActivation = false;

        while (loadAsyncOperation.progress < 0.9f)
        {
            await Task.Yield();
        }

        loadAsyncOperation.allowSceneActivation = true;

        while (!loadAsyncOperation.isDone)
        {
            await Task.Yield();
        }

        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);

            await InitializeSceneAsync(newScene);

            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }

        loadedSceneBySlot[sceneSlot] = sceneName;
    }

    private async Task InitializeSceneAsync(Scene scene)
    {
        var rootObjects = scene.GetRootGameObjects();

        List<Task> tasks = new();

        foreach (GameObject root in rootObjects)
        {
            var components = root.GetComponentsInChildren<ISceneInitializable>(true);

            foreach (ISceneInitializable component in components)
            {
                tasks.Add(component.InitializeAsync());
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task UnloadSceneAsync(SceneSlot sceneSlot)
    {
        if (!loadedSceneBySlot.TryGetValue(sceneSlot, out string sceneName))
        {
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        AsyncOperation unloadAsyncOperation = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadAsyncOperation != null)
        {
            while (!unloadAsyncOperation.isDone)
            {
                await Task.Yield();
            }
        }

        loadedSceneBySlot.Remove(sceneSlot);
    }

    private async Task CleanupUnusedAssetsAsync()
    {
        AsyncOperation cleanupUnusedAssetsAsyncOperation = Resources.UnloadUnusedAssets();

        while (!cleanupUnusedAssetsAsyncOperation.isDone)
        {
            await Task.Yield();
        }
    }

    public class SceneTransitionPlan
    {
        internal Dictionary<SceneSlot, string> ScenesToLoad { get; } = new();
        internal List<SceneSlot> ScenesToUnload { get; } = new();
        public string ActiveSceneName { get; private set; } = string.Empty;
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool UseOverlay { get; private set; } = false;
        public InputMode? TargetInputMode { get; private set; }

        public SceneTransitionPlan Load(SceneSlot sceneSlot, string sceneName, bool setActive = false)
        {
            ScenesToLoad[sceneSlot] = sceneName;

            if (setActive)
            {
                ActiveSceneName = sceneName;
            }

            return this;
        }

        public SceneTransitionPlan Unload(SceneSlot sceneSlot)
        {
            ScenesToUnload.Add(sceneSlot);
            return this;
        }

        public SceneTransitionPlan WithOverlay()
        {
            UseOverlay = true;
            return this;
        }

        public SceneTransitionPlan WithInputMode(InputMode mode)
        {
            TargetInputMode = mode;
            return this;
        }

        public SceneTransitionPlan WithClearUnusedAssets()
        {
            ClearUnusedAssets = true;
            return this;
        }

        public Task Perform()
        {
            return SceneController.Instance.ExecutePlan(this);
        }
    }
}

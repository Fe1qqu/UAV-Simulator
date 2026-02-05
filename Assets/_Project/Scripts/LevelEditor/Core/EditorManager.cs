using UnityEngine;
using UnityEngine.Localization.Components;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Manages level editor UI: category list, placeable object list, and scene loading.
/// </summary>
public class EditorManager : MonoBehaviour, IBackHandler
{
    [SerializeField] private LevelFileManager levelFileManager;
    [SerializeField] private LevelLoader levelLoader;

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;

    [SerializeField] private EditorPauseMenu pauseMenu;

    [Tooltip("Parent transform where category buttons will be instantiated.")]
    [SerializeField] private Transform categoryListContainer;

    [Tooltip("Parent transform where placeable object buttons will be instantiated.")]
    [SerializeField] private Transform objectListContainer;

    [Tooltip("LocalizeStringEvent attached to currentCategoryLabel.")]
    [SerializeField] private LocalizeStringEvent currentCategoryLocalizeEvent;

    [Header("Prefabs")]
    [Tooltip("Prefab used for constructing a category button in the category list.")]
    [SerializeField] private GameObject categoryButtonPrefab;

    [Tooltip("Prefab used for constructing a placeable object button in the object list.")]
    [SerializeField] private GameObject placeableObjectButtonPrefab;

    [Header("Databases")]
    [Tooltip("Database containing all available locations.")]
    [SerializeField] private LocationDatabase locationDatabase;

    [Tooltip("Database that stores all placeable objects available to the editor.")]
    [SerializeField] private PlaceableObjectDatabase placeableObjectDatabase;

    [Tooltip("Database that stores object categories and their icons.")]
    [SerializeField] private CategoryDatabase categoryDatabase;

    [SerializeField] private ScenarioDatabase scenarioDatabase;

    [Header("Scene Root")]
    [Tooltip("Parent under which the level and placed objects will be instantiated.")]
    [SerializeField] private Transform levelRoot;

    // Currently selected category button
    private UICategoryButton currentSelectedButton;

    // Active category enum value
    private PlaceableObjectType currentCategory;

    private FadeManager loadingScreenFader;

    private LocalizationPreloader localizationPreloader;

    public LevelFileManager LevelFileManager => levelFileManager;

    public ScenarioDefinition CurrentScenario => scenarioDatabase.GetById(GameSettings.Instance.CurrentEditorSession.SelectedScenarioId);

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[EditorManager] LevelFileManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[EditorManager] LevelLoader is not assigned.");
        }
   
        if (loadingScreen == null)
        {
            Debug.LogError("[EditorManager] LoadingScreen is not assigned.");
        }

        if (pauseMenu == null)
        {
            Debug.LogError("[EditorManager] PauseMenu is not assigned.");
        }

        if (categoryListContainer == null)
        {
            Debug.LogError("[EditorManager] CategoryListContainer is not assigned.");
        }

        if (objectListContainer == null)
        {
            Debug.LogError("[EditorManager] ObjectListContainer is not assigned.");
        }

        if (currentCategoryLocalizeEvent == null)
        {
            Debug.LogWarning("[EditorManager] CurrentCategoryLocalizeEvent is not assigned.");
        }

        if (categoryButtonPrefab == null)
        {
            Debug.LogError("[EditorManager] CategoryButtonPrefab is not assigned.");
        }

        if (placeableObjectButtonPrefab == null)
        {
            Debug.LogError("[EditorManager] PlaceableObjectButtonPrefab is not assigned.");
        }

        if (locationDatabase == null || locationDatabase.locations.Count == 0)
        {
            Debug.LogError("[EditorManager] LocationDatabase is missing or empty.");
        }

        if (placeableObjectDatabase == null || placeableObjectDatabase.objects.Count == 0)
        {
            Debug.LogError("[EditorManager] PlaceableObjectDatabase is missing or empty.");
        }

        if (categoryDatabase == null || categoryDatabase.categories.Count == 0)
        {
            Debug.LogError("[EditorManager] CategoryDatabase is missing or empty.");
        }

        if (scenarioDatabase == null || scenarioDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[EditorManager] ScenarioDatabase is missing or empty.");
        }

        if (levelRoot == null)
        {
            Debug.LogError("[EditorManager] LevelRoot is not assigned.");
        }

        loadingScreenFader = loadingScreen.GetComponent<FadeManager>();
        if (loadingScreenFader == null)
        {
            Debug.LogError("[EditorManager] FadeManager not found on loadingScreen.");
        }

        localizationPreloader = GetComponent<LocalizationPreloader>();
        if (localizationPreloader == null)
        {
            Debug.LogError("[EditorManager] LocalizationPreloader not found on this GameObject.");
        }
    }

    private void Start()
    {
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        loadingScreen.SetActive(true);

        try
        {
            await InitializeEditorAsync();
            await loadingScreenFader.FadeOutAsync(0.35f, destroyCancellationToken);
        }
        finally
        {
            loadingScreen.SetActive(false);
        }
    }

    private async Task InitializeEditorAsync()
    {
        Task loadTask = localizationPreloader.Load();

        LoadLevelOrEmpty();
        SetupCategories();

        await loadTask;
    }

    /// <summary>
    /// Creates UI buttons for each category from the database.
    /// </summary>
    private void SetupCategories()
    {
        foreach (Transform child in categoryListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CategoryData categoryData in categoryDatabase.categories)
        {
            GameObject categoryButtonInstance = Instantiate(categoryButtonPrefab, categoryListContainer);
            if (!categoryButtonInstance.TryGetComponent<UICategoryButton>(out var categoryButton))
            {
                Debug.LogError("[EditorManager] CategoryButtonPrefab missing UICategoryButton component.");
                continue;
            }

            categoryButtonInstance.SetActive(true);
            categoryButton.Setup(categoryData, OnCategorySelected);
        }

        // Auto–select first category
        if (categoryDatabase.categories.Count > 0)
        {
            CategoryData firstCategory = categoryDatabase.categories[0];
            UICategoryButton firstButton = categoryListContainer.GetChild(0).GetComponent<UICategoryButton>();
            OnCategorySelected(firstCategory, firstButton);
        }
    }

    /// <summary>
    /// Called when a category button is clicked.
    /// Updates UI and object list.
    /// </summary>
    private void OnCategorySelected(CategoryData categoryData, UICategoryButton categoryButton)
    {
        if (currentSelectedButton != null)
        {
            currentSelectedButton.SetSelected(false);
        }

        currentSelectedButton = categoryButton;
        currentSelectedButton.SetSelected(true);

        currentCategory = categoryData.type;
        if (currentCategoryLocalizeEvent != null)
        {
            currentCategoryLocalizeEvent.StringReference = categoryData.localizationKey;
            currentCategoryLocalizeEvent.RefreshString();
        }

        RefreshObjectList();
    }

    /// <summary>
    /// Rebuilds the list of buttons for objects of the selected category.
    /// </summary>
    void RefreshObjectList()
    {
        foreach (Transform child in objectListContainer)
        {
            Destroy(child.gameObject);
        }

        var filteredObjects = placeableObjectDatabase.GetByType(currentCategory);

        if (filteredObjects == null || filteredObjects.Count == 0)
        {
            Debug.LogWarning($"[EditorManager] No objects found for category '{currentCategory}'.");
            return;
        }

        foreach (PlaceableObjectData placeableObjectData in filteredObjects)
        {
            GameObject placeableObjectButtonInstance = Instantiate(placeableObjectButtonPrefab, objectListContainer);
            if (!placeableObjectButtonInstance.TryGetComponent<UIPlaceableObjectButton>(out var placeableObjectButton))
            {
                Debug.LogError("[EditorManager] PlaceableObjectButtonPrefab missing UIPlaceableObjectButton.");
                continue;
            }

            placeableObjectButtonInstance.SetActive(true);
            placeableObjectButton.Setup(placeableObjectData);
        }
    }

    private void LoadLevelOrEmpty()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;
        string levelFilePath = editorSession.SelectedLevelFilePath;

        if (string.IsNullOrEmpty(levelFilePath) || !File.Exists(levelFilePath))
        {
            Debug.Log("[EditorManager] Creating new empty level.");
            LoadEmptyLevel();
            return;
        }

        LevelData levelData = levelFileManager.LoadByPath(editorSession.SelectedLevelFilePath);
        levelLoader.Load(levelData);

        editorSession.LevelName = levelData.levelName;
        editorSession.SelectedLocationId = levelData.locationId;
    }

    private void LoadEmptyLevel()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData empty = new()
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId
        };

        levelLoader.Load(empty);
    }

    public bool OnBack()
    {
        Debug.Log("[EditorManager] OnBack.");
        pauseMenu.Open();
        return true;
    }

    public void UnloadLocalization()
    {
        if (localizationPreloader != null)
        {
            localizationPreloader.Unload();
        }
    }
}

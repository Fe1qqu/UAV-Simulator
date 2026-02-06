using UnityEngine;
using UnityEngine.Localization.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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
    /// Creates UI buttons for each category from the database with scenario rules.
    /// </summary>
    private void SetupCategories()
    {
        foreach (Transform child in categoryListContainer)
        {
            Destroy(child.gameObject);
        }

        ScenarioDefinition scenario = CurrentScenario;
        if (scenario == null)
        {
            Debug.LogWarning("[EditorManager] No scenario selected. Showing all categories.");
        }

        List<CategoryData> categoriesToShow = GetAvailableCategories(scenario);

        // Create category buttons
        foreach (CategoryData categoryData in categoriesToShow)
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
        if (categoriesToShow.Count > 0)
        {
            CategoryData firstCategoryData = categoriesToShow[0];
            UICategoryButton firstCategoryButton = categoryListContainer.GetChild(0).GetComponent<UICategoryButton>();
            OnCategorySelected(firstCategoryData, firstCategoryButton);
        }
    }

    private List<CategoryData> GetAvailableCategories(ScenarioDefinition scenario)
    {
        List<CategoryData> categoriesToShow = new();

        foreach (CategoryData categoryData in categoryDatabase.categories)
        {
            // If the scenario is selected, check the rules
            if (scenario != null)
            {
                bool categoryAllowed = scenario.availableCategories.Exists(rule => rule.category == categoryData.type);
                if (!categoryAllowed)
                {
                    continue;
                }
            }

            categoriesToShow.Add(categoryData);
        }

        return categoriesToShow;
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
            //currentCategoryLocalizeEvent.RefreshString();
        }

        RefreshObjectList();
    }

    /// <summary>
    /// Rebuilds the list of buttons for objects of the selected category with scenario rules.
    /// </summary>
    void RefreshObjectList()
    {
        foreach (Transform child in objectListContainer)
        {
            Destroy(child.gameObject);
        }

        // Take all objects of the selected category
        List<PlaceableObjectData> filteredObjects = placeableObjectDatabase.GetByType(currentCategory);
        if (filteredObjects == null || filteredObjects.Count == 0)
        {
            Debug.LogWarning($"[EditorManager] No objects found for category '{currentCategory}'.");
            return;
        }

        ScenarioDefinition scenario = CurrentScenario;
        if (scenario != null)
        {
            ScenarioCategoryRule rule = scenario.availableCategories.Find(rule => rule.category == currentCategory);
            filteredObjects = FilterObjectsByScenarioRule(filteredObjects, rule);
        }

        // Create buttons for objects
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

    private List<PlaceableObjectData> FilterObjectsByScenarioRule(List<PlaceableObjectData> objects, ScenarioCategoryRule rule)
    {
        if (rule == null || objects == null)
        {
            return objects;
        }

        // Check for empty list in modes that require it
        if ((rule.accessMode == ScenarioCategoryAccessMode.ListedOnly || rule.accessMode == ScenarioCategoryAccessMode.AllExceptListed)
            && (rule.objectIds == null || rule.objectIds.Count == 0))
        {
            Debug.LogWarning($"[EditorManager] ScenarioCategoryRule for category '{rule.category}' has accessMode '{rule.accessMode}' but objectIds list is empty.");
        }

        // Check for invalid objectIds
        if (rule.objectIds != null && rule.objectIds.Count > 0)
        {
            foreach (string objectId in rule.objectIds)
            {
                bool exists = objects.Exists(obj => obj.objectId == objectId);
                if (!exists)
                {
                    Debug.LogWarning($"[EditorManager] ScenarioCategoryRule for category '{rule.category}' references objectId '{objectId}', but it was not found in the database.");
                }
            }
        }

        // Filtering by accessMode
        return rule.accessMode switch
        {
            ScenarioCategoryAccessMode.All => objects,
            ScenarioCategoryAccessMode.ListedOnly => objects.FindAll(obj => rule.objectIds.Contains(obj.objectId)),
            ScenarioCategoryAccessMode.AllExceptListed => objects.FindAll(obj => !rule.objectIds.Contains(obj.objectId)),
            _ => objects
        };
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
        editorSession.SelectedScenarioId = levelData.scenarioId;
    }

    private void LoadEmptyLevel()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData empty = new()
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId,
            scenarioId = editorSession.SelectedScenarioId
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

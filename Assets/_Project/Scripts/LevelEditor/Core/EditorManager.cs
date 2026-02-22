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

    [Header("Localization")]
    [SerializeField] private LocalizationPreloader localizationPreloader;

    // Currently selected category button
    private UICategoryButton currentSelectedButton;

    private CategoryDefinition currentCategory;

    private FadeManager loadingScreenFader;

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

        if (localizationPreloader == null)
        {
            Debug.LogError("[EditorManager] LocalizationPreloader is not assigned.");
        }

        loadingScreenFader = loadingScreen.GetComponent<FadeManager>();
        if (loadingScreenFader == null)
        {
            Debug.LogError("[EditorManager] FadeManager not found on loadingScreen.");
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

        List<CategoryDefinition> categoriesToShow = GetAvailableCategories(scenario);

        // Create category buttons
        foreach (CategoryDefinition category in categoriesToShow)
        {
            GameObject categoryButtonInstance = Instantiate(categoryButtonPrefab, categoryListContainer);
            if (!categoryButtonInstance.TryGetComponent<UICategoryButton>(out var categoryButton))
            {
                Debug.LogError("[EditorManager] CategoryButtonPrefab missing UICategoryButton component.");
                continue;
            }

            categoryButtonInstance.SetActive(true);
            categoryButton.Setup(category, OnCategorySelected);
        }

        // Auto–select first category
        if (categoriesToShow.Count > 0)
        {
            CategoryDefinition firstCategory = categoriesToShow[0];
            UICategoryButton firstCategoryButton = categoryListContainer.GetChild(0).GetComponent<UICategoryButton>();
            OnCategorySelected(firstCategory, firstCategoryButton);
        }
    }

    private List<CategoryDefinition> GetAvailableCategories(ScenarioDefinition scenario)
    {
        List<CategoryDefinition> categoriesToShow = new();

        foreach (CategoryDefinition category in categoryDatabase.categories)
        {
            // If the scenario is selected, check the rules
            if (scenario != null)
            {
                bool categoryAllowed = scenario.availableCategories.Exists(rule => rule.category == category);
                if (!categoryAllowed)
                {
                    continue;
                }
            }

            categoriesToShow.Add(category);
        }

        return categoriesToShow;
    }

    /// <summary>
    /// Called when a category button is clicked.
    /// Updates UI and object list.
    /// </summary>
    private void OnCategorySelected(CategoryDefinition category, UICategoryButton categoryButton)
    {
        if (currentSelectedButton != null)
        {
            currentSelectedButton.SetSelected(false);
        }

        currentSelectedButton = categoryButton;
        currentSelectedButton.SetSelected(true);

        currentCategory = category;
        if (currentCategoryLocalizeEvent != null)
        {
            currentCategoryLocalizeEvent.StringReference = category.localizedString;
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
        List<PlaceableObjectDefinition> filteredPlaceableObjects = placeableObjectDatabase.GetByCategory(currentCategory);
        if (filteredPlaceableObjects == null || filteredPlaceableObjects.Count == 0)
        {
            Debug.LogWarning($"[EditorManager] No objects found for category '{currentCategory}'.");
            return;
        }

        ScenarioDefinition scenario = CurrentScenario;
        if (scenario != null)
        {
            ScenarioCategoryRule rule = scenario.availableCategories.Find(rule => rule.category == currentCategory);
            filteredPlaceableObjects = FilterObjectsByScenarioRule(filteredPlaceableObjects, rule);
        }

        // Create buttons for objects
        foreach (PlaceableObjectDefinition placeableObject in filteredPlaceableObjects)
        {
            GameObject placeableObjectButtonInstance = Instantiate(placeableObjectButtonPrefab, objectListContainer);
            if (!placeableObjectButtonInstance.TryGetComponent<UIPlaceableObjectButton>(out var placeableObjectButton))
            {
                Debug.LogError("[EditorManager] PlaceableObjectButtonPrefab missing UIPlaceableObjectButton.");
                continue;
            }

            placeableObjectButtonInstance.SetActive(true);
            placeableObjectButton.Setup(placeableObject);
        }
    }

    private List<PlaceableObjectDefinition> FilterObjectsByScenarioRule(List<PlaceableObjectDefinition> placeableObjects, ScenarioCategoryRule scenarioCategoryRule)
    {
        if (scenarioCategoryRule == null || placeableObjects == null)
        {
            return placeableObjects;
        }

        // Check for empty list in modes that require it
        if ((scenarioCategoryRule.accessMode == ScenarioCategoryAccessMode.ListedOnly || scenarioCategoryRule.accessMode == ScenarioCategoryAccessMode.AllExceptListed)
            && (scenarioCategoryRule.objectIds == null || scenarioCategoryRule.objectIds.Count == 0))
        {
            Debug.LogWarning($"[EditorManager] ScenarioCategoryRule for category '{scenarioCategoryRule.category}' has accessMode '{scenarioCategoryRule.accessMode}' but objectIds list is empty.");
        }

        bool usesObjectList = scenarioCategoryRule.accessMode != ScenarioCategoryAccessMode.All;

        // Check for invalid objectIds
        if (usesObjectList && scenarioCategoryRule.objectIds != null && scenarioCategoryRule.objectIds.Count > 0)
        {
            foreach (string objectId in scenarioCategoryRule.objectIds)
            {
                bool exists = placeableObjects.Exists(obj => obj.objectId == objectId);
                if (!exists)
                {
                    Debug.LogWarning($"[EditorManager] ScenarioCategoryRule for category '{scenarioCategoryRule.category}' references objectId '{objectId}', but it was not found in the database.");
                }
            }
        }

        // Filtering by accessMode
        return scenarioCategoryRule.accessMode switch
        {
            ScenarioCategoryAccessMode.All => placeableObjects,
            ScenarioCategoryAccessMode.ListedOnly => placeableObjects.FindAll(obj => scenarioCategoryRule.objectIds.Contains(obj.objectId)),
            ScenarioCategoryAccessMode.AllExceptListed => placeableObjects.FindAll(obj => !scenarioCategoryRule .objectIds.Contains(obj.objectId)),
            _ => placeableObjects
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

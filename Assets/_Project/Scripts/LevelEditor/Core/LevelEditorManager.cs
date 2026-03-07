using UnityEngine;
using UnityEngine.Localization.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// Manages level editor UI: category list, placeable object list, and scene loading.
/// </summary>
public class LevelEditorManager : MonoBehaviour, IBackHandler
{
    [SerializeField] private LevelFileManager levelFileManager;
    [SerializeField] private LevelLoader levelLoader;

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;

    [SerializeField] private LevelEditorPauseMenu levelEditorPauseMenu;

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
    [SerializeField] private LocationsDatabase locationsDatabase;

    [Tooltip("Database that stores all placeable objects available to the level editor.")]
    [SerializeField] private PlaceableObjectsDatabase placeableObjectsDatabase;

    [Tooltip("Database that stores object categories and their icons.")]
    [SerializeField] private CategoriesDatabase categoriesDatabase;

    [SerializeField] private ScenariosDatabase scenariosDatabase;

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

    public ScenarioDefinition CurrentScenario => scenariosDatabase.GetById(GameSettings.Instance.CurrentLevelEditorSession.SelectedScenarioId);

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[LevelEditorManager] LevelFileManager is not assigned.");
        }

        if (levelLoader == null)
        {
            Debug.LogError("[LevelEditorManager] LevelLoader is not assigned.");
        }
   
        if (loadingScreen == null)
        {
            Debug.LogError("[LevelEditorManager] LoadingScreen is not assigned.");
        }

        if (levelEditorPauseMenu == null)
        {
            Debug.LogError("[LevelEditorManager] LevelEditorPauseMenu is not assigned.");
        }

        if (categoryListContainer == null)
        {
            Debug.LogError("[LevelEditorManager] CategoryListContainer is not assigned.");
        }

        if (objectListContainer == null)
        {
            Debug.LogError("[LevelEditorManager] ObjectListContainer is not assigned.");
        }

        if (currentCategoryLocalizeEvent == null)
        {
            Debug.LogWarning("[LevelEditorManager] CurrentCategoryLocalizeEvent is not assigned.");
        }

        if (categoryButtonPrefab == null)
        {
            Debug.LogError("[LevelEditorManager] CategoryButtonPrefab is not assigned.");
        }

        if (placeableObjectButtonPrefab == null)
        {
            Debug.LogError("[LevelEditorManager] PlaceableObjectButtonPrefab is not assigned.");
        }

        if (locationsDatabase == null || locationsDatabase.locations.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] LocationsDatabase is missing or empty.");
        }

        if (placeableObjectsDatabase == null || placeableObjectsDatabase.objects.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] PlaceableObjectsDatabase is missing or empty.");
        }

        if (categoriesDatabase == null || categoriesDatabase.categories.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] CategoriesDatabase is missing or empty.");
        }

        if (scenariosDatabase == null || scenariosDatabase.scenarios.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] ScenariosDatabase is missing or empty.");
        }

        if (levelRoot == null)
        {
            Debug.LogError("[LevelEditorManager] LevelRoot is not assigned.");
        }

        if (localizationPreloader == null)
        {
            Debug.LogError("[LevelEditorManager] LocalizationPreloader is not assigned.");
        }

        loadingScreenFader = loadingScreen.GetComponent<FadeManager>();
        if (loadingScreenFader == null)
        {
            Debug.LogError("[LevelEditorManager] FadeManager not found on loadingScreen.");
        }
    }

    private void Start()
    {
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        InputModeController.Instance.SetMode(InputMode.Loading);

        loadingScreen.SetActive(true);

        try
        {
            await InitializeLevelEditorAsync();
            await loadingScreenFader.FadeOutAsync(0.35f, destroyCancellationToken);
        }
        finally
        {
            loadingScreen.SetActive(false);

            InputModeController.Instance.SetMode(InputMode.LevelEditor);
        }
    }

    private async Task InitializeLevelEditorAsync()
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
            Debug.LogWarning("[LevelEditorManager] No scenario selected. Showing all categories.");
        }

        List<CategoryDefinition> categoriesToShow = GetAvailableCategories(scenario);

        // Create category buttons
        foreach (CategoryDefinition category in categoriesToShow)
        {
            GameObject categoryButtonInstance = Instantiate(categoryButtonPrefab, categoryListContainer);
            if (!categoryButtonInstance.TryGetComponent<UICategoryButton>(out var categoryButton))
            {
                Debug.LogError("[LevelEditorManager] CategoryButtonPrefab missing UICategoryButton component.");
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

        foreach (CategoryDefinition category in categoriesDatabase.categories)
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
        List<PlaceableObjectDefinition> filteredPlaceableObjects = placeableObjectsDatabase.GetByCategory(currentCategory);
        if (filteredPlaceableObjects == null || filteredPlaceableObjects.Count == 0)
        {
            Debug.LogWarning($"[LevelEditorManager] No objects found for category '{currentCategory}'.");
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
                Debug.LogError("[LevelEditorManager] PlaceableObjectButtonPrefab missing UIPlaceableObjectButton.");
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
            Debug.LogWarning($"[LevelEditorManager] ScenarioCategoryRule for category '{scenarioCategoryRule.category}' has accessMode '{scenarioCategoryRule.accessMode}' but objectIds list is empty.");
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
                    Debug.LogWarning($"[LevelEditorManager] ScenarioCategoryRule for category '{scenarioCategoryRule.category}' references objectId '{objectId}', but it was not found in the database.");
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
        LevelEditorSession levelEditorSession = GameSettings.Instance.CurrentLevelEditorSession;
        string levelFilePath = levelEditorSession.SelectedLevelFilePath;

        if (string.IsNullOrEmpty(levelFilePath) || !File.Exists(levelFilePath))
        {
            Debug.Log("[LevelEditorManager] Creating new empty level.");
            LoadEmptyLevel();
            return;
        }

        LevelData levelData = levelFileManager.LoadByPath(levelEditorSession.SelectedLevelFilePath);
        levelLoader.Load(levelData);

        levelEditorSession.LevelName = levelData.levelName;
        levelEditorSession.SelectedScenarioId = levelData.scenarioId;
    }

    private void LoadEmptyLevel()
    {
        LevelEditorSession levelEditorSession = GameSettings.Instance.CurrentLevelEditorSession;

        LevelData empty = new()
        {
            levelName = levelEditorSession.LevelName,
            locationId = levelEditorSession.SelectedLocationId,
            scenarioId = levelEditorSession.SelectedScenarioId
        };

        levelLoader.Load(empty);
    }

    public bool OnBack()
    {
        //Debug.Log("[LevelEditorManager] OnBack.");
        levelEditorPauseMenu.Open();
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

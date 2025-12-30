using UnityEngine;
using UnityEngine.Localization.Components;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Manages level editor UI: category list, placeable object list, and scene loading.
/// </summary>
public class EditorManager : MonoBehaviour, IBackHandler
{
    [SerializeField] private LevelSaveManager levelSaveManager;
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

    [Header("Scene Root")]
    [Tooltip("Parent under which the level and placed objects will be instantiated.")]
    [SerializeField] private Transform levelRoot;

    // Currently selected category button
    private UICategoryButton currentSelectedButton;

    // Active category enum value
    private PlaceableObjectType currentCategory;

    private FadeManager loadingFader;

    private EditorLocalizationPreloader localizationPreloader;

    private void Awake()
    {
        if (levelSaveManager == null)
        {
            Debug.LogError("[EditorManager] LevelSaveManager is not assigned.");
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

        if (levelRoot == null)
        {
            Debug.LogError("[EditorManager] LevelRoot is not assigned.");
        }

        loadingFader = loadingScreen.GetComponent<FadeManager>();
        if (loadingFader == null)
        {
            Debug.LogError("[EditorManager] FadeManager not found on loadingScreen.");
        }

        localizationPreloader = GetComponent<EditorLocalizationPreloader>();
        if (localizationPreloader == null)
        {
            Debug.LogError("[EditorManager] EditorLocalizationPreloader not found on this GameObject.");
        }
    }

    private async void Start()
    {
        loadingScreen.SetActive(true);

        await InitializeEditorAsync();

        await loadingFader.FadeOutAsync(0.35f);
        loadingScreen.SetActive(false);
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

        foreach (CategoryData category in categoryDatabase.categories)
        {
            var buttonObj = Instantiate(categoryButtonPrefab, categoryListContainer);
            
            if (!buttonObj.TryGetComponent<UICategoryButton>(out var categoryButton))
            {
                Debug.LogError("[EditorManager] CategoryButtonPrefab missing UICategoryButton");
                continue;
            }

            categoryButton.Setup(category, OnCategorySelected);
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
    private void OnCategorySelected(CategoryData category, UICategoryButton button)
    {
        if (currentSelectedButton != null)
        {
            currentSelectedButton.SetSelected(false);
        }

        currentSelectedButton = button;
        currentSelectedButton.SetSelected(true);

        currentCategory = category.type;
        if (currentCategoryLocalizeEvent != null)
        {
            currentCategoryLocalizeEvent.StringReference = category.localizationKey;
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

        foreach (PlaceableObjectData objData in filteredObjects)
        {
            GameObject buttonObj = Instantiate(placeableObjectButtonPrefab, objectListContainer);
            if (!buttonObj.TryGetComponent<UIPlaceableObjectButton>(out var button))
            {
                Debug.LogError("[EditorManager] PlaceableObjectButtonPrefab missing UIPlaceableObjectButton!");
                continue;
            }

            button.Setup(objData);
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

        LevelData data = levelSaveManager.LoadByPath(editorSession.SelectedLevelFilePath);
        levelLoader.Load(data);

        editorSession.LevelName = data.levelName;
        editorSession.SelectedLocationId = data.locationId;
    }

    private void LoadEmptyLevel()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData empty = new LevelData
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId
        };

        levelLoader.Load(empty);
    }

    /// <summary>
    /// Loads the location chosen earlier in GameSettings.
    /// </summary>
    //private void LoadSelectedLocation()
    //{
    //    EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;
    //    string selectedLocationId = editorSession.SelectedLocationId;

    //    if (string.IsNullOrEmpty(selectedLocationId))
    //    {
    //        Debug.LogWarning("[EditorManager] No selected location found. Loading first available location.");

    //        if (locationDatabase.locations.Count == 0)
    //        {
    //            Debug.LogError("[EditorManager] LocationDatabase is empty.");
    //            return;
    //        }

    //        selectedLocationId = locationDatabase.locations[0].locationId;
    //        editorSession.SelectedLocationId = selectedLocationId;
    //    }

    //    LocationData data = locationDatabase.locations.Find(location => location.locationId == selectedLocationId);
    //    if (data == null)
    //    {
    //        Debug.LogWarning($"[EditorManager] Location with Id '{selectedLocationId}' not found in database. Loading default.");
    //        data = locationDatabase.locations[0];
    //    }

    //    if (data.prefab == null)
    //    {
    //        Debug.LogError($"[EditorManager] Prefab missing for location '{data.localizationKey}'.");
    //        return;
    //    }

    //    Instantiate(data.prefab, Vector3.zero, Quaternion.identity, levelRoot);
    //    //currentLocation = Instantiate(data.prefab, Vector3.zero, Quaternion.identity, levelRoot);
    //    //Debug.Log($"[EditorManager] Loaded location: {location.name}");
    //}

    public bool OnBack()
    {
        Debug.Log("[EditorManager] OnBack.");
        pauseMenu.Open();
        return true;
    }

    //public void ExitEditor()
    //{
    //    localizationPreloader.Unload();
    //    SceneManager.LoadScene("MainMenu");
    //}
}

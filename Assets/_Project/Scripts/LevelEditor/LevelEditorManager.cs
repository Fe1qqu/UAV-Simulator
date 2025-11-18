using UnityEngine;
using UnityEngine.Localization.Components;
using System.Threading.Tasks;

/// <summary>
/// Manages level editor UI: category list, placeable object list, and scene loading.
/// </summary>
public class LevelEditorManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;

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
        if (loadingScreen == null)
        {
            Debug.LogError("[LevelEditorManager] loadingScreen is not assigned.");
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
            Debug.LogWarning("[LevelEditorManager] currentCategoryLocalizeEvent is not assigned.");
        }

        if (categoryButtonPrefab == null)
        {
            Debug.LogError("[LevelEditorManager] CategoryButtonPrefab is not assigned.");
        }

        if (placeableObjectButtonPrefab == null)
        {
            Debug.LogError("[LevelEditorManager] PlaceableObjectButtonPrefab is not assigned.");
        }

        if (locationDatabase == null || locationDatabase.locations.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] LocationDatabase is missing or empty.");
        }

        if (placeableObjectDatabase == null || placeableObjectDatabase.objects.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] PlaceableObjectDatabase is missing or empty.");
        }

        if (categoryDatabase == null || categoryDatabase.categories.Count == 0)
        {
            Debug.LogError("[LevelEditorManager] CategoryDatabase is missing or empty.");
        }

        if (levelRoot == null)
        {
            Debug.LogError("[LevelEditorManager] LevelRoot is not assigned.");
        }

        loadingFader = loadingScreen.GetComponent<FadeManager>();
        if (loadingFader == null)
        {
            Debug.LogError("[LevelEditorManager] FadeManager not found on loadingScreen.");
        }

        localizationPreloader = GetComponent<EditorLocalizationPreloader>();
        if (localizationPreloader == null)
        {
            Debug.LogError("[LevelEditorManager] EditorLocalizationPreloader not found on this GameObject.");
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

        LoadSelectedLocation();
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

        foreach (var category in categoryDatabase.categories)
        {
            var buttonObj = Instantiate(categoryButtonPrefab, categoryListContainer);
            
            if (!buttonObj.TryGetComponent<UICategoryButton>(out var categoryButton))
            {
                Debug.LogError("[LevelEditorManager] CategoryButtonPrefab missing UICategoryButton");
                continue;
            }

            categoryButton.Setup(category, OnCategorySelected);
        }

        // Auto–select first category
        if (categoryDatabase.categories.Count > 0)
        {
            var firstCategory = categoryDatabase.categories[0];
            var firstButton = categoryListContainer.GetChild(0).GetComponent<UICategoryButton>();
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
            Debug.LogWarning($"[LevelEditorManager] No objects found for category '{currentCategory}'.");
            return;
        }

        foreach (var objData in filteredObjects)
        {
            var buttonObj = Instantiate(placeableObjectButtonPrefab, objectListContainer);
            if (!buttonObj.TryGetComponent<UIPlaceableObjectButton>(out var button))
            {
                Debug.LogError("[LevelEditorManager] PlaceableObjectButtonPrefab missing UIPlaceableObjectButton!");
                continue;
            }

            button.Setup(objData);
        }
    }

    /// <summary>
    /// Loads the location chosen earlier in GameSettings.
    /// </summary>
    private void LoadSelectedLocation()
    {
        long selectedLocationId = GameSettings.Instance.SelectedLocationId;

        if (selectedLocationId == 0 && locationDatabase.locations.Count > 0)
        {
            Debug.LogWarning("[LevelEditorManager] No selected location found. Loading first available location.");
            selectedLocationId = locationDatabase.locations[0].localizationKey.TableEntryReference.KeyId;
            GameSettings.Instance.SelectedLocationId = selectedLocationId;
        }

        var data = locationDatabase.locations.Find(location => location.localizationKey.TableEntryReference.KeyId == selectedLocationId);
        if (data == null)
        {
            Debug.LogWarning($"[LevelEditorManager] Location with Id '{selectedLocationId}' not found in database. Loading default.");
            data = locationDatabase.locations[0];
        }

        if (data.prefab == null)
        {
            Debug.LogError($"[LevelEditorManager] Prefab missing for location '{data.localizationKey}'.");
            return;
        }

        Instantiate(data.prefab, Vector3.zero, Quaternion.identity, levelRoot);
        //currentLocation = Instantiate(data.prefab, Vector3.zero, Quaternion.identity, levelRoot);
        //Debug.Log($"[LevelEditorManager] Loaded location: {location.name}");
    }

    //public void ExitEditor()
    //{
    //    localizationPreloader.Unload();
    //    SceneManager.LoadScene("MainMenu");
    //}
}

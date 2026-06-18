using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Databases")]
    [SerializeField] private CategoriesDatabase categoriesDatabase;
    [SerializeField] private LocationsDatabase locationsDatabase;
    [SerializeField] private PlaceableObjectsDatabase placeableObjectsDatabase;
    [SerializeField] private ScenariosDatabase scenariosDatabase;

    public CategoriesDatabase Categories => categoriesDatabase;
    public LocationsDatabase Locations => locationsDatabase;
    public PlaceableObjectsDatabase PlaceableObjects => placeableObjectsDatabase;
    public ScenariosDatabase Scenarios => scenariosDatabase;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[GameDataManager] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (categoriesDatabase == null)
        {
            Debug.LogError("[GameDataManager] CategoriesDatabase is not assigned.");
        }

        if (locationsDatabase == null)
        {
            Debug.LogError("[GameDataManager] LocationsDatabase is not assigned.");
        }

        if (placeableObjectsDatabase == null)
        {
            Debug.LogError("[GameDataManager] PlaceableObjectsDatabase is not assigned.");
        }

        if (scenariosDatabase == null)
        {
            Debug.LogError("[GameDataManager] ScenariosDatabase is not assigned.");
        }
    }
}

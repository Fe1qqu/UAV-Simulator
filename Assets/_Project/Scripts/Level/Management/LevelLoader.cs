using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[LevelLoader] LevelRoot is not assigned.");
        }
    }

    public void Load(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("[LevelLoader] LevelData is null.");
            return;
        }

        ClearLevel();
        LoadLocation(levelData.locationId);
        LoadObjects(levelData);
    }

    private void ClearLevel()
    {
        foreach (Transform child in levelRoot)
        {
            Destroy(child.gameObject);
        }
    }

    private void LoadLocation(string locationId)
    {
        LocationDefinition location = GameDataManager.Instance.Locations.GetById(locationId);

        if (location == null)
        {
            location = GameDataManager.Instance.Locations.locations[0];

            Debug.LogWarning($"[LevelLoader] Location '{locationId}' not found. Falling back to default location '{location.locationId}'.");
        }
        
        Instantiate(location.prefab, Vector3.zero, Quaternion.identity, levelRoot);
    }

    private void LoadObjects(LevelData levelData)
    {
        foreach (LevelObjectData levelObjectData in levelData.objects)
        {
            PlaceableObjectDefinition placeableObject = GameDataManager.Instance.PlaceableObjects.GetById(levelObjectData.objectId);
            if (placeableObject == null)
            {
                Debug.LogError($"[LevelLoader] Missing object: {levelObjectData.objectId}.");
                continue;
            }

            GameObject placeableObjectInstance = Instantiate(placeableObject.prefab, levelRoot);
            LevelObject levelObject = placeableObjectInstance.GetComponent<LevelObject>();

            levelObject.Initialize(placeableObject);
            levelObject.FromData(levelObjectData);
        }
    }
}

using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private LocationDatabase locationDatabase;
    [SerializeField] private PlaceableObjectDatabase placeableObjectDatabase;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[LevelLoader] LevelRoot is not assigned.");
        }

        if (locationDatabase == null)
        {
            Debug.LogError("[LevelLoader] LocationDatabase is not assigned.");
        }

        if (placeableObjectDatabase == null)
        {
            Debug.LogError("[LevelLoader] PlaceableObjectDatabase is not assigned.");
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
        LocationData location = locationDatabase.locations.Find(location => location.locationId == locationId);

        if (location == null)
        {
            if (locationDatabase.locations == null || locationDatabase.locations.Count == 0)
            {
                Debug.LogError("[LevelLoader] LocationDatabase is empty.");
                return;
            }

            location = locationDatabase.locations[0];

            Debug.LogWarning($"[LevelLoader] Location '{locationId}' not found. Falling back to default location '{location.locationId}'.");
        }
        
        Instantiate(location.prefab, Vector3.zero, Quaternion.identity, levelRoot);
    }

    private void LoadObjects(LevelData data)
    {
        foreach (LevelObjectData levelObjectData in data.objects)
        {
            PlaceableObjectData placeableObjectData = placeableObjectDatabase.GetById(levelObjectData.objectId);
            if (placeableObjectData == null)
            {
                Debug.LogError($"[LevelLoader] Missing object: {levelObjectData.objectId}.");
                continue;
            }

            GameObject instance = Instantiate(placeableObjectData.prefab, levelRoot);
            LevelObject levelObject = instance.GetComponent<LevelObject>();

            levelObject.Initialize(placeableObjectData);
            levelObject.FromData(levelObjectData);
        }
    }
}

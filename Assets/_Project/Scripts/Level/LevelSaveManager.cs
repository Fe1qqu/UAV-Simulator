using UnityEngine;
using System.IO;

public class LevelSaveManager : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private PlaceableObjectDatabase placeableObjectDatabase;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[LevelSaveManager] LevelRoot is not assigned.");
        }

        if (placeableObjectDatabase == null)
        {
            Debug.LogError("[LevelSaveManager] PlaceableObjectDatabase is not assigned.");
        }
    }

    public void Save(string fileName)
    {
        //if (!LevelValidator.Validate(out var error))
        //{
        //    Debug.LogError($"[LevelSaveManager] {error}");
        //    return;
        //}

        LevelData data = Collect();
        string json = JsonUtility.ToJson(data, true);

        string path = GetPath(fileName);
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);

        Debug.Log($"[LevelSaveManager] Level saved to {path}.");
    }

    public void Load(string fileName)
    {
        string path = GetPath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("[LevelSaveManager] Level file not found.");
            return;
        }

        string json = File.ReadAllText(path);
        LevelData data = JsonUtility.FromJson<LevelData>(json);

        ApplyLevelData(data);
    }

    private LevelData Collect()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData data = new LevelData
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId
        };

        foreach (Transform child in levelRoot)
        {
            if (!child.TryGetComponent<LevelObject>(out var obj))
            {
                continue;
            }

            data.objects.Add(obj.ToData());
        }

        return data;
    }

    private void ApplyLevelData(LevelData data)
    {
        foreach (Transform child in levelRoot)
        {
            if (child.GetComponent<LevelObject>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (LevelObjectData obj in data.objects)
        {
            PlaceableObjectData placeableObjectData = placeableObjectDatabase.GetById(obj.objectId);
            if (placeableObjectData == null)
            {
                Debug.LogError($"[LevelSaveManager] Missing object: {obj.objectId}.");
                continue;
            }

            GameObject instance = Instantiate(placeableObjectData.prefab, levelRoot);
            LevelObject levelObj = instance.GetComponent<LevelObject>();
            levelObj.FromData(obj);
        }
    }

    private string GetPath(string name)
    {
        return Path.Combine(Application.persistentDataPath, "levels", name + ".json");
    }
}

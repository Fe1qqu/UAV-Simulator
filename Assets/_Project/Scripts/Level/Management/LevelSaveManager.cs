using UnityEngine;
using System.IO;

public class LevelSaveManager : MonoBehaviour
{
    public void SaveByPath(string path, LevelData data)
    {
        //if (!LevelValidator.Validate(out var error))
        //{
        //    Debug.LogError($"[LevelSaveManager] {error}.");
        //    return;
        //}

        string json = JsonUtility.ToJson(data, true);
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);

        Debug.Log($"[LevelSaveManager] Level saved to {path}.");
    }

    public LevelData LoadByPath(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[LevelSaveManager] Level file not found: {path}.");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<LevelData>(json);
    }
}

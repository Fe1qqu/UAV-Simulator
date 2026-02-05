using UnityEngine;
using System.IO;

public class LevelFileManager : MonoBehaviour
{
    public void SaveByPath(string path, LevelData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);

        Debug.Log($"[LevelFileManager] Level saved to {path}.");
    }

    public LevelData LoadByPath(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[LevelFileManager] Level file not found: {path}.");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<LevelData>(json);
    }
}

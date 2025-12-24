using UnityEngine;

/// <summary>
/// Centralized settings manager for game-wide data.
/// </summary>
public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                var existing = FindFirstObjectByType<GameSettings>();
                if (existing != null)
                {
                    _instance = existing;
                }
                else
                {
                    GameObject obj = new GameObject("GameSettings");
                    _instance = obj.AddComponent<GameSettings>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // -------------------------------
    // === LEVEL CREATION SETTINGS ===
    // -------------------------------

    /// <summary>
    /// Name of the level currently being created or edited.
    /// </summary>
    public string LevelName
    {
        get => PlayerPrefs.GetString("LevelName");
        set
        {
            PlayerPrefs.SetString("LevelName", value);
            Save();
        }
    }

    /// <summary>
    /// Selected location ID for level creation.
    /// </summary>
    public string SelectedLocationId
    {
        get => PlayerPrefs.GetString("SelectedLocationId");
        set
        {
            PlayerPrefs.SetString("SelectedLocationId", value);
            Save();
        }
    }

    // -----------------------
    // === GENERAL SETTINGS ===
    // -----------------------

    //public float MasterVolume
    //{
    //    get => PlayerPrefs.GetFloat("MasterVolume", 1f);
    //    set
    //    {
    //        PlayerPrefs.SetFloat("MasterVolume", Mathf.Clamp01(value));
    //        Save();
    //    }
    //}

    //public bool IsFullscreen
    //{
    //    get => PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
    //    set
    //    {
    //        PlayerPrefs.SetInt("IsFullscreen", value ? 1 : 0);
    //        Save();
    //    }
    //}

    // -----------------------
    // === CORE METHODS ===
    // -----------------------

    public void Save()
    {
        PlayerPrefs.Save();
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("[GameSettings] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Clear temporary level creation settings at startup
        PlayerPrefs.DeleteKey("LevelName");
        PlayerPrefs.DeleteKey("SelectedLocationId");
    }
}

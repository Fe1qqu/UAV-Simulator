using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;

public class PlaySession
{
    public string LevelFilePath;

    public void Clear()
    {
        LevelFilePath = null;
    }
}

public class EditorSession
{
    public string LevelName;
    public string SelectedLocationId;
    public string SelectedScenarioId;
    public string SelectedLevelFilePath;

    public void Clear()
    {
        LevelName = null;
        SelectedLocationId = null;
        SelectedScenarioId = null;
        SelectedLevelFilePath = null;
    }
}

/// <summary>
/// Centralized settings manager for game-wide data.
/// </summary>
public class GameSettings : MonoBehaviour
{
    #region Constants

    private const string PREF_LANGUAGE_CODE = "LanguageCode";
    private const string DEFAULT_LANGUAGE_CODE = "ru";

    private const string PREF_VSYNC = "VSync";

    #endregion

    #region Singleton

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
                    GameObject obj = new("GameSettings");
                    _instance = obj.AddComponent<GameSettings>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    #endregion

    #region Runtime Sessions (Not Persisted)

    public PlaySession CurrentPlaySession { get; private set; } = new();
    public EditorSession CurrentEditorSession { get; private set; } = new();

    public void ClearPlaySession() => CurrentPlaySession.Clear();
    public void ClearEditorSession() => CurrentEditorSession.Clear();

    #endregion

    #region Localization

    private TaskCompletionSource<bool> _localizationReadyTaskCompletionSource = new();
    public Task LocalizationReadyTask => _localizationReadyTaskCompletionSource.Task;

    #endregion

    #region Unity Lifecycle

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
    }

    private void Start()
    {
        ApplyVSync(SavedVSyncEnabled);
        _ = InitializeLocalizationAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeLocalizationAsync()
    {
        await LocalizationSettings.InitializationOperation.Task;

        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(SavedLanguageCode);
        if (locale == null)
        {
            locale = LocalizationSettings.SelectedLocale;
        }

        LocalizationSettings.SelectedLocale = locale;

        _localizationReadyTaskCompletionSource.TrySetResult(true);
    }

    #endregion

    #region Public API

    public void SetLocale(Locale locale)
    {
        if (locale == null)
        {
            return;
        }

        LocalizationSettings.SelectedLocale = locale;
        SavedLanguageCode = locale.Identifier.Code;
    }

    public bool VSyncEnabled => SavedVSyncEnabled;

    public void SetVSync(bool enabled)
    {
        ApplyVSync(enabled);
        SavedVSyncEnabled = enabled;
    }

    #endregion

    #region Properties

    private string SavedLanguageCode
    {
        get => PlayerPrefs.GetString(PREF_LANGUAGE_CODE, DEFAULT_LANGUAGE_CODE);
        set
        {
            PlayerPrefs.SetString(PREF_LANGUAGE_CODE, value);
            Save();
        }
    }

    private bool SavedVSyncEnabled
    {
        get => PlayerPrefs.GetInt(PREF_VSYNC, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(PREF_VSYNC, value ? 1 : 0);
            Save();
        }
    }

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

    #endregion

    #region Utilities

    private void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        Application.targetFrameRate = enabled ? -1 : 60;
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    #endregion
}

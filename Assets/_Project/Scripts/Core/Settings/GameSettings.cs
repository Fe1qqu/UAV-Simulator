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
    public string SelectedLevelFilePath;

    public void Clear()
    {
        LevelName = null;
        SelectedLocationId = null;
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
                    GameObject obj = new GameObject("GameSettings");
                    _instance = obj.AddComponent<GameSettings>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    #endregion

    #region Runtime Sessions (Not Persisted)

    public PlaySession CurrentPlaySession { get; private set; } = new PlaySession();
    public EditorSession CurrentEditorSession { get; private set; } = new EditorSession();

    public void ClearPlaySession() => CurrentPlaySession.Clear();
    public void ClearEditorSession() => CurrentEditorSession.Clear();

    #endregion

    #region Localization

    private TaskCompletionSource<bool> _localizationReadyTcs = new TaskCompletionSource<bool>();
    public Task LocalizationReadyTask => _localizationReadyTcs.Task;

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

        _localizationReadyTcs.TrySetResult(true);
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

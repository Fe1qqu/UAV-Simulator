using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized settings manager for game-wide data.
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [SerializeField] private SettingsDatabase settingsDatabase;

    [Header("Quality Levels")]
    [SerializeField] private int mainMenuQualityLevel = 0;
    [SerializeField] private int gameplayQualityLevel = 1;

    private readonly Dictionary<string, SettingInstance> settings = new();
    private readonly Dictionary<SettingAutoApply, List<SettingInstance>> settingsByAutoApply = new();

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[GameSettings] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (settingsDatabase == null)
        {
            Debug.LogError("[GameSettings] SettingsDatabase is not assigned.");
            return;
        }

        Initialize();
    }

    private void OnEnable()
    {
        GameStateManager.StateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.StateChanged -= OnGameStateChanged;
    }

    //private void Update()
    //{
    //    //SettingInstance settingInstance = Get("fps_limit");
    //    //print($"{settingInstance.GetValue()}");

    //    print(Application.targetFrameRate);
    //}

    #endregion

    #region Initialization

    private void Initialize()
    {
        settings.Clear();
        settingsByAutoApply.Clear();

        foreach (SettingDefinition setting in settingsDatabase.Settings)
        {
            if (string.IsNullOrWhiteSpace(setting.Id))
            {
                Debug.LogError("[GameSettings] SettingDefinition has empty settingId.");
                continue;
            }

            if (settings.ContainsKey(setting.Id))
            {
                Debug.LogError($"[GameSettings] Duplicate settingId detected: {setting.Id}.");
                continue;
            }

            SettingInstance settingInstance = new(setting, Get);
            
            settings.Add(setting.Id, settingInstance);

            SettingAutoApply autoApply = setting.AutoApply;
            if (!settingsByAutoApply.TryGetValue(autoApply, out var scopedList))
            {
                scopedList = new List<SettingInstance>();
                settingsByAutoApply.Add(autoApply, scopedList);
            }
            scopedList.Add(settingInstance);
        }
    }

    #endregion

    #region Mode Switching

    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                EnterMainMenu();
                break;

            case GameState.Gameplay:
                EnterGameplay();
                break;
        }
    }

    private void EnterMainMenu()
    {
        SetMainMenuQuality();
        Application.targetFrameRate = 60;
    }

    private void EnterGameplay()
    {
        SetGameplayQuality();
        ApplyAuto(SettingAutoApply.OnEnterGameplay);
    }

    private void SetMainMenuQuality()
    {
        QualitySettings.SetQualityLevel(mainMenuQualityLevel, false);
    }

    private void SetGameplayQuality()
    {
        QualitySettings.SetQualityLevel(gameplayQualityLevel, false);
    }

    #endregion

    #region Settings Access

    public SettingInstance Get(string id)
    {
        settings.TryGetValue(id, out var setting);
        return setting;
    }

    //public void SetRuntimeValue(SettingInstance setting, object value)
    //{
    //    setting.SetRuntimeValue(value);
    //}

    public void Apply(SettingInstance setting)
    {
        setting.Apply();
    }

    public void Apply(IEnumerable<SettingInstance> settings)
    {
        foreach (SettingInstance setting in settings)
        {
            setting.Apply();
        }
    }

    public void Save(SettingInstance setting)
    {
        setting.Save();
        PlayerPrefs.Save();
    }

    public void Save(IEnumerable<SettingInstance> settings)
    {
        foreach (SettingInstance setting in settings)
        {
            setting.Save();
        }

        PlayerPrefs.Save();
    }

    public void ChangeApplyAndSave(SettingInstance setting, object value)
    {
        setting.SetRuntimeValue(value);
        setting.Apply();
        setting.Save();
        PlayerPrefs.Save();
    }

    public void ApplyAuto(SettingAutoApply autoApply)
    {
        if (!settingsByAutoApply.TryGetValue(autoApply, out var settings))
        {
            return;
        }

        foreach (SettingInstance setting in settings)
        {
            setting.Apply();
        }
    }

    #endregion

    #region Utilities

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    #endregion
}

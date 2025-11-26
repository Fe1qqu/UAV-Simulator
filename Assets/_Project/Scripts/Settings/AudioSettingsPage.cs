using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Пример страницы "Audio". Настраивает MasterVolume через GameSettings.
/// </summary>
public class AudioSettingsPage : SettingsPage
{
    [Header("UI")]
    [SerializeField] private Slider masterVolumeSlider;

    private void Awake()
    {
        if (masterVolumeSlider == null)
        {
            Debug.LogWarning("[AudioSettingsPage] masterVolumeSlider not assigned.");
        }
    }

    public override void OnPageSelected()
    {
        //base.OnPageSelected();

        // load current value
        //if (masterVolumeSlider != null)
        //{
        //    // предполагается, что GameSettings имеет свойство MasterVolume; если нет — добавь
        //    masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        //    masterVolumeSlider.onValueChanged.RemoveAllListeners();
        //    masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        //}
    }

    public override void OnPageUnselected()
    {
        //base.OnPageUnselected();

        //if (masterVolumeSlider != null)
        //{
        //    masterVolumeSlider.onValueChanged.RemoveAllListeners();
        //}
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
        Debug.Log($"[AudioSettingsPage] MasterVolume set to {value}");
    }
}

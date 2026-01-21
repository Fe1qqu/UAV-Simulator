using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsTab : SettingsTabBase
{
    [Header("UI")]
    [SerializeField] private Slider masterVolumeSlider;

    protected override void Awake()
    {
        base.Awake();

        if (masterVolumeSlider == null)
        {
            Debug.LogWarning("[AudioSettingsTab] masterVolumeSlider not assigned.");
        }
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        // load current value
        //if (masterVolumeSlider != null)
        //{
        //    // предполагаетс€, что GameSettings имеет свойство MasterVolume; если нет Ч добавь
        //    masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        //    masterVolumeSlider.onValueChanged.RemoveAllListeners();
        //    masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        //}
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();

        //if (masterVolumeSlider != null)
        //{
        //    masterVolumeSlider.onValueChanged.RemoveAllListeners();
        //}
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
        Debug.Log($"[AudioSettingsTab] MasterVolume set to {value}");
    }
}

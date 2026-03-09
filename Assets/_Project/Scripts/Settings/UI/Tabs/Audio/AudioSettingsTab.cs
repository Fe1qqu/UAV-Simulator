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
    }

    public override void OnTabUnselected()
    {
        base.OnTabUnselected();
    }
}

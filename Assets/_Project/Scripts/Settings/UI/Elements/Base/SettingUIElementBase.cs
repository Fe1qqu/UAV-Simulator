using UnityEngine;
using Alchemy.Inspector;
using TMPro;

public abstract class SettingUIElementBase : MonoBehaviour
{
    public enum VisibilityMode
    {
        DisableGameObject,
        DisableInteraction,
        FadeAndDisableInteraction
    }

    [Header("Visibility")]
    [SerializeField] private VisibilityMode visibilityMode = VisibilityMode.DisableGameObject;

    private bool UsesCanvasGroup => visibilityMode == VisibilityMode.FadeAndDisableInteraction;

    [ShowIf(nameof(UsesCanvasGroup))]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI")]
    [SerializeField] private TMP_Text labelText;

    protected SettingInstance boundSetting;
    protected SettingDefinition definition;

    public SettingInstance BoundSetting => boundSetting as SettingInstance;

    protected virtual void Awake()
    {
        if (visibilityMode == VisibilityMode.FadeAndDisableInteraction && canvasGroup == null)
        {
            Debug.LogError("[SettingUIElementBase] CanvasGroup required for Fade mode.");
        }

        if (labelText == null)
        {
            Debug.LogError("[SettingUIElementBase] LabelText is not assigned.");
        }
    }

    public virtual void Bind(SettingInstance setting)
    {
        Unbind();

        boundSetting = setting;

        if (boundSetting is not SettingInstance instance)
        {
            Debug.LogError($"[{GetType().Name}] Setting must be SettingInstance.");
            return;
        }

        definition = instance.Definition;

        if (definition.displayName != null)
        {
            definition.displayName.StringChanged += OnDisplayNameChanged;
            labelText.text = definition.displayName.GetLocalizedString();
        }

        boundSetting.OnValueChanged += OnSettingValueChanged;
        instance.OnVisibilityChanged += ApplyVisibility;

        ApplyVisibility(instance.IsVisible);
    }

    protected virtual void Unbind()
    {
        if (boundSetting is SettingInstance instance)
        {
            instance.OnVisibilityChanged -= ApplyVisibility;
        }

        if (boundSetting != null)
        {
            boundSetting.OnValueChanged -= OnSettingValueChanged;
        }
        
        if (definition != null && definition.displayName != null)
        {
            definition.displayName.StringChanged -= OnDisplayNameChanged;
        }
    }

    private void OnDisplayNameChanged(string value)
    {
        labelText.text = value;
    }

    private void ApplyVisibility(bool visible)
    {
        switch (visibilityMode)
        {
            case VisibilityMode.DisableGameObject:
                gameObject.SetActive(visible);
                break;

            case VisibilityMode.DisableInteraction:
                SetInteractable(visible);
                break;

            case VisibilityMode.FadeAndDisableInteraction:
                canvasGroup.alpha = visible ? 1f : 0.35f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
                break;
        }
    }

    protected virtual void SetInteractable(bool interactable) { }

    protected abstract void OnSettingValueChanged(object value);

    protected virtual void OnDestroy()
    {
        Unbind();
    }
}

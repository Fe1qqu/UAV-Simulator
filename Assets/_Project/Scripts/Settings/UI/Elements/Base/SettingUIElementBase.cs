using UnityEngine;
using Alchemy.Inspector;
using TMPro;

public abstract class SettingUIElementBase : MonoBehaviour
{
    public enum VisibilityMode
    {
        AlwaysVisible,
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

    public SettingInstance BoundSetting => boundSetting;

    private bool isBound;

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
        if (isBound)
        {
            Unbind();
        }

        boundSetting = setting;
        definition = boundSetting.Definition;

        isBound = true;

        if (definition.displayName != null)
        {
            definition.displayName.StringChanged += OnDisplayNameChanged;
            labelText.text = definition.displayName.GetLocalizedString();
        }

        boundSetting.OnRuntimeValueChanged += OnRuntimeValueChanged;
        boundSetting.OnVisibilityChanged += ApplyVisibility;

        ApplyVisibility(boundSetting.IsVisible);
    }

    protected virtual void Unbind()
    {
        if (!isBound)
        {
            return;
        }    

        if (boundSetting != null)
        {
            boundSetting.OnRuntimeValueChanged -= OnRuntimeValueChanged;
            boundSetting.OnVisibilityChanged -= ApplyVisibility;
        }

        if (definition != null && definition.displayName != null)
        {
            definition.displayName.StringChanged -= OnDisplayNameChanged;
        }

        boundSetting = null;
        definition = null;
        isBound = false;
    }

    public void Reload()
    {
        boundSetting.Reload();
    }

    private void OnRuntimeValueChanged(object value)
    {
        Refresh();
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

    protected abstract void Refresh();

    protected virtual void OnDestroy()
    {
        Unbind();
    }
}

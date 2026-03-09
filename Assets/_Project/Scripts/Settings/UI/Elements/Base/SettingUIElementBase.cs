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

    [Header("Reference")]
    [SerializeField] private SettingDefinition definitionReference;

    [Header("Visibility")]
    [SerializeField] private VisibilityMode visibilityMode = VisibilityMode.DisableGameObject;

    private bool UsesCanvasGroup => visibilityMode == VisibilityMode.FadeAndDisableInteraction;

    [ShowIf(nameof(UsesCanvasGroup))]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI")]
    [SerializeField] private TMP_Text labelText;

    protected SettingInstance boundSetting;
    public SettingInstance BoundSetting => boundSetting;

    private bool isBound;

    protected virtual void Awake()
    {
        if (definitionReference == null)
        {
            Debug.LogError("[SettingUIElementBase] DefinitionReference is not assigned.");
        }

        if (visibilityMode == VisibilityMode.FadeAndDisableInteraction && canvasGroup == null)
        {
            Debug.LogError("[SettingUIElementBase] CanvasGroup required for Fade mode.");
        }

        if (labelText == null)
        {
            Debug.LogError("[SettingUIElementBase] LabelText is not assigned.");
        }
    }

    public void AutoBind()
    {
        SettingInstance instance = GameSettings.Instance.Get(definitionReference.Id);
        if (instance != null)
        {
            Bind(instance);
        }
        else
        {
            Debug.LogError($"[SettingUIElementBase] {name}: SettingInstance with ID '{definitionReference.Id}' not found.");
        }
    }

    protected virtual void Bind(SettingInstance setting)
    {
        if (isBound)
        {
            Unbind();
        }

        if (setting == null)
        {
            Debug.LogError("[SettingUIElementBase] Cannot bind to null SettingInstance.");
            return;
        }

        boundSetting = setting;

        isBound = true;

        if (boundSetting.Definition.DisplayName != null)
        {
            boundSetting.Definition.DisplayName.StringChanged += OnDisplayNameChanged;
            labelText.text = boundSetting.Definition.DisplayName.GetLocalizedString();
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

        if (boundSetting.Definition != null && boundSetting.Definition.DisplayName != null)
        {
            boundSetting.Definition.DisplayName.StringChanged -= OnDisplayNameChanged;
        }

        boundSetting = null;
        isBound = false;
    }

    public void Reload()
    {
        boundSetting?.Reload();
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

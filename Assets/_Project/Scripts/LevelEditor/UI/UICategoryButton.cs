using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UICategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource
{
    [Header("References")]
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform tooltipAnchor;
    [SerializeField] private UISelectionButtonVisual visual;

    private CategoryDefinition linkedCategory;

    private System.Action<CategoryDefinition, UICategoryButton> onClick;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (icon == null)
        {
            Debug.LogWarning($"[UICategoryButton] Icon is not assigned on '{name}'.");
        }

        if (visual == null)
        {
            Debug.LogWarning($"[UICategoryButton] Visual is not assigned on '{name}'.");
        }

        if (tooltipAnchor == null)
        {
            Debug.LogWarning($"[UICategoryButton] TooltipAnchor is not assigned.");
        }
    }

    public void Setup(CategoryDefinition category, System.Action<CategoryDefinition, UICategoryButton> onClick)
    {
        if (category == null)
        {
            Debug.LogError("[UICategoryButton] Tried to setup with null Category.");
            return;
        }

        linkedCategory = category;
        this.onClick = onClick;

        icon.sprite = linkedCategory.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(linkedCategory, this));

        SetSelected(false);
    }

    /// <summary>
    /// Applies visual selection state by adjusting transparency.
    /// </summary>
    /// <param name="selected">Whether this category is currently selected.</param>
    public void SetSelected(bool selected)
    {
        visual.SetSelected(selected);
    }

    /// <summary>
    /// Called when the pointer enters the button area.
    /// Shows the tooltip for this category unless data is missing.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.Show(this);
    }

    /// <summary>
    /// Called when the pointer exits the button area.
    /// Hides the tooltip unless drag mode is active.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance.IsInDragMode)
        {
            return;
        }

        TooltipManager.Instance.Hide();
    }

    public TooltipRequest CreateTooltipRequest()
    {
        if (linkedCategory == null)
        {
            Debug.LogWarning("[UICategoryButton] Tried to create tooltip request with null Category.");
            return TooltipRequest.Invalid;
        }

        return new TooltipRequest
        {
            isValid = true,
            text = linkedCategory.localizedString,
            explicitSettings = linkedCategory.useTooltipSettingsOverride ? linkedCategory.tooltipSettingsOverride : null,
            context = gameObject,
            fixedAnchor = tooltipAnchor
        };
    }
}

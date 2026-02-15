using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI button representing a single category inside the level editor.
/// Handles visual selection, tooltip display, and click events.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class UICategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource
{
    // Button component attached to this UI element
    private Button button;

    // Icon image shown inside the category button
    private Image icon;

    // CanvasGroup used to visually highlight selected state
    private CanvasGroup canvasGroup;

    // Category data assigned during initialization
    private CategoryDefinition linkedCategory;

    // Callback invoked when this button is clicked
    private System.Action<CategoryDefinition, UICategoryButton> onClick;

    [SerializeField] private RectTransform tooltipAnchor;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        icon = transform.Find("Icon").GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogWarning($"[UICategoryButton] Missing icon on {name}.");
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
        canvasGroup.alpha = selected ? 0.6f : 1.0f;
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
            text = linkedCategory.localizationKey,
            explicitSettings = linkedCategory.useTooltipSettingsOverride ? linkedCategory.tooltipSettingsOverride : null,
            context = gameObject,
            fixedAnchor = tooltipAnchor
        };
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Localization;

/// <summary>
/// UI button representing a single category inside the level editor.
/// Handles visual selection, tooltip display, and click events.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class UICategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Button component attached to this UI element
    private Button button;

    // Icon image shown inside the category button
    private Image icon;

    // CanvasGroup used to visually highlight selected state
    private CanvasGroup canvasGroup;

    // Category data assigned during initialization
    [HideInInspector] public CategoryData categoryData;

    // Callback invoked when this button is clicked
    private System.Action<CategoryData, UICategoryButton> onClick;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        icon = transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogWarning($"[UICategoryButton] Missing Icon on {name}");
        }
    }

    /// <summary>
    /// Sets up the category button with category data and a click callback.
    /// </summary>
    /// <param name="data">Category data containing icon and display name.</param>
    /// <param name="onClick">Callback invoked when the button is pressed.</param>
    public void Setup(CategoryData data, System.Action<CategoryData, UICategoryButton> onClick)
    {
        if (data == null)
        {
            Debug.LogError("[UICategoryButton] Tried to setup with null CategoryData.");
            return;
        }

        categoryData = data;
        this.onClick = onClick;

        if (icon != null)
        {
            icon.sprite = data.icon;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(data, this));

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
        if (categoryData == null)
        {
            return; 
        }

        TooltipManager.Instance.Show(categoryData.localizationKey);
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
}

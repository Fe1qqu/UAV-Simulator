using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI button that represents a single placeable object.
/// Supports tooltip display, drag-to-place functionality, and drag visual feedback.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class UIPlaceableObjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ITooltipSource
{
    [Tooltip("Object data that defines icon, prefab, and type.")]
    [HideInInspector] public PlaceableObjectData linkedObjectData;

    // Icon image used to represent the placeable object
    private Image icon;

    // CanvasGroup used to visually indicate drag state
    private CanvasGroup canvasGroup;

    void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogWarning($"[UIPlaceableObjectButton] Icon not found on '{name}'.");
        }

        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Sets up this button with placeable object data.
    /// Assigns icon and stores reference for dragging.
    /// </summary>
    /// <param name="data">Placeable object data.</param>
    public void Setup(PlaceableObjectData data)
    {
        if (data == null)
        {
            Debug.LogError("[UIPlaceableObjectButton] Tried to setup with null PlaceableObjectData.");
            return;
        }

        linkedObjectData = data;
        icon.sprite = data.icon;
    }

    /// <summary>
    /// Called when drag begins. Starts placement drag preview and modifies UI state.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UIDragContext.Instance == null)
        {
            Debug.LogError("[UIPlaceableObjectButton] UIDragContext not found. Drag cancelled.");
            return;
        }

        if (linkedObjectData == null || linkedObjectData.prefab == null)
        {
            Debug.LogError("[UIPlaceableObjectButton] linkedObjectData or its prefab is null. Drag cancelled.");
            return;
        }

        Debug.Log($"[UIPlaceableObjectButton] Begin drag {linkedObjectData.localizationKey}");

        TooltipManager.Instance.EnterDragMode();
        DragPlacementHandler.Instance.BeginDrag(linkedObjectData);

        ApplyDragVisual();
        DragPlacementHandler.Instance.OnDragCancelled += ResetDragVisual;
    }

    /// <summary>
    /// Called during drag. Updates the UI drag context zones.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (UIDragContext.Instance != null)
        {
            UIDragContext.Instance.UpdateContext(eventData);
        }
    }

    /// <summary>
    /// Finalizes the drag operation and restores the button's normal state.
    /// Determines whether the action should be completed or cancelled based on the pointer position.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[UIPlaceableObjectButton] End drag.");

        TooltipManager.Instance.ExitDragMode();

        ResetDragVisual();

        if (UIDragContext.Instance != null && UIDragContext.Instance.IsPointerOverCancelZone)
        {
            Debug.Log("[UIPlaceableObjectButton] Drag cancelled (released over cancel zone).");
            DragPlacementHandler.Instance.CancelDrag();
        }
        else
        {
            DragPlacementHandler.Instance.EndDrag(eventData);
        }

        DragPlacementHandler.Instance.OnDragCancelled -= ResetDragVisual;
    }

    /// <summary>
    /// Applies visual feedback when drag begins.
    /// </summary>
    private void ApplyDragVisual()
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Restores normal UI appearance after drag ends.
    /// </summary>
    private void ResetDragVisual()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Called when pointer enters button area.
    /// Displays tooltip unless drag mode is active.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (linkedObjectData == null)
        {
            Debug.LogWarning("[UIPlaceableObjectButton] Tried to show tooltip with null linkedObjectData.");
            return;
        }

        TooltipManager.Instance.Show(this);
    }

    /// <summary>
    /// Called when pointer exits button area.
    /// Hides tooltip unless drag mode is active.
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
        if (linkedObjectData == null)
        {
            Debug.LogWarning("[UIPlaceableObjectButton] Tried to create tooltip request with null linkedObjectData.");
            return TooltipRequest.Invalid;
        }

        return new TooltipRequest
        {
            isValid = true,
            text = linkedObjectData.localizationKey,
            explicitSettings = linkedObjectData.useTooltipSettingsOverride ? linkedObjectData.tooltipSettingsOverride : null,
            context = gameObject,
        };
    }
}

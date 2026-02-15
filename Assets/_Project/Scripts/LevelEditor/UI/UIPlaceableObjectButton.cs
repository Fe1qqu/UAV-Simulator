using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI button that represents a single placeable object.
/// Supports tooltip display, drag-to-place functionality, and drag visual feedback.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class UIPlaceableObjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ITooltipSource
{
    [HideInInspector] public PlaceableObjectDefinition linkedPlaceableObject;

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

    public void Setup(PlaceableObjectDefinition placeableObject)
    {
        if (placeableObject == null)
        {
            Debug.LogError("[UIPlaceableObjectButton] Tried to setup with null PlaceableObject.");
            return;
        }

        linkedPlaceableObject = placeableObject;
        icon.sprite = placeableObject.icon;
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

        if (linkedPlaceableObject == null || linkedPlaceableObject.prefab == null)
        {
            Debug.LogError("[UIPlaceableObjectButton] linkedPlaceableObject or its prefab is null. Drag cancelled.");
            return;
        }

        Debug.Log($"[UIPlaceableObjectButton] Begin drag '{linkedPlaceableObject.objectId}'.");

        TooltipManager.Instance.EnterDragMode();
        DragPlacementHandler.Instance.BeginDrag(linkedPlaceableObject);

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
        if (linkedPlaceableObject == null)
        {
            Debug.LogWarning("[UIPlaceableObjectButton] Tried to show tooltip with null PlaceableObject.");
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
        if (linkedPlaceableObject == null)
        {
            Debug.LogWarning("[UIPlaceableObjectButton] Tried to create tooltip request with null PlaceableObject.");
            return TooltipRequest.Invalid;
        }

        return new TooltipRequest
        {
            isValid = true,
            text = linkedPlaceableObject.localizationKey,
            explicitSettings = linkedPlaceableObject.useTooltipSettingsOverride ? linkedPlaceableObject.tooltipSettingsOverride : null,
            context = gameObject,
        };
    }
}

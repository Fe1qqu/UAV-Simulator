using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Manages the context of UI drag operations, tracking which UI zones the pointer is over.
/// Handles cancel zones and shows tooltips for relevant UI areas.
/// </summary>
public class UIDragContext : MonoBehaviour, ITooltipSource
{
    public static UIDragContext Instance { get; private set; }

    [Header("Drag Zones")]
    [Tooltip("List of UI zones that interact with dragging.")]
    [SerializeField] private List<DragZoneContext> dragZoneContexts = new();

    /// <summary>
    /// True if the pointer is currently over a drag cancel zone.
    /// </summary>
    public bool IsPointerOverCancelZone { get; private set; }

    // Current drag context zone the pointer is over.
    private DragZoneContext currentZone;

    // Current game object the pointer is over.
    private GameObject currentContext;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[UIDragContext] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Updates the current drag context based on pointer event data.
    /// Tracks the current UI zone and shows/hides tooltips accordingly.
    /// </summary>
    /// <param name="eventData">Pointer event data from the UI system.</param>
    public void UpdateContext(PointerEventData eventData)
    {
        currentContext = eventData.pointerEnter;
        DragZoneContext dragContextZone = null;

        if (currentContext != null)
        {
            foreach (DragZoneContext zoneContext in dragZoneContexts)
            {
                if (IsPointerOverTaggedParent(currentContext, zoneContext.tag))
                {
                    dragContextZone = zoneContext;
                    break;
                }
            }
        }

        if (dragContextZone != currentZone)
        {
            if (currentZone != null)
            {
                TooltipManager.Instance.Hide();
            }

            currentZone = dragContextZone;

            if (currentZone != null && currentZone.tooltipLocalizedKey != null)
            {
                TooltipManager.Instance.Show(this);
            }
        }

        IsPointerOverCancelZone = currentZone != null && currentZone.cancelsDrag;
    }

    /// <summary>
    /// Checks if the pointer is over a GameObject (or any of its parents) with the specified tag.
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <param name="tag">The tag to match.</param>
    /// <returns>True if the object or any parent has the tag.</returns>
    private bool IsPointerOverTaggedParent(GameObject obj, string tag)
    {
        while (obj != null)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }

            if (obj.transform.parent == null)
            {
                break;
            }

            obj = obj.transform.parent.gameObject;
        }
        return false;
    }

    /// <summary>
    /// Resets the drag context and hides any active tooltip.
    /// Should be called when drag ends or is cancelled.
    /// </summary>
    public void ResetContext()
    {
        TooltipManager.Instance.Hide();

        IsPointerOverCancelZone = false;
        currentZone = null;
    }

    public TooltipRequest CreateTooltipRequest()
    {
        if (currentZone == null)
        {
            Debug.LogWarning("[UIDragContext] Tooltip requested but CurrentZone is null.");
            return TooltipRequest.Invalid;
        }

        if (currentContext == null)
        {
            Debug.LogWarning("[UIDragContext] Tooltip requested but currentContext is null.");
            return TooltipRequest.Invalid;
        }

        return new TooltipRequest
        {
            isValid = true,
            text = currentZone.tooltipLocalizedKey,
            explicitSettings = currentZone.tooltipSettings,
            context = currentContext,
            force = true
        };
    }
}

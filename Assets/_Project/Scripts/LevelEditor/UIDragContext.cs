using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Manages the context of UI drag operations, tracking which UI zones the pointer is over.
/// Handles cancel zones and shows tooltips for relevant UI areas.
/// </summary>
public class UIDragContext : MonoBehaviour
{
    public static UIDragContext Instance { get; private set; }

    [Header("Drag Zones")]
    [Tooltip("List of UI zones that interact with dragging.")]
    [SerializeField] private List<DragContextZone> dragZones = new List<DragContextZone>();

    /// <summary>
    /// True if the pointer is currently over a drag cancel zone.
    /// </summary>
    public bool IsPointerOverCancelZone { get; private set; }

    /// <summary>
    /// Current drag context zone the pointer is over.
    /// </summary>
    public DragContextZone CurrentZone { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[UIDragContext] Duplicate instance detected. There should only be one in the scene.");
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
        var target = eventData.pointerEnter;
        DragContextZone dragContextZone = null;

        if (target != null)
        {
            foreach (var zone in dragZones)
            {
                if (IsPointerOverTaggedParent(target, zone.tag))
                {
                    dragContextZone = zone;
                    break;
                }
            }
        }

        if (dragContextZone != CurrentZone)
        {
            if (CurrentZone != null)
            {
                TooltipManager.Instance.Hide();
            }

            CurrentZone = dragContextZone;

            if (CurrentZone?.tooltipLocalizedKey != null)
            {
                TooltipManager.Instance.Show(CurrentZone.tooltipLocalizedKey, force: true);
            }
        }

        IsPointerOverCancelZone = CurrentZone != null && CurrentZone.cancelsDrag;
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
        CurrentZone = null;
    }
}

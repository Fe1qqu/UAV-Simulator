using UnityEngine;
using UnityEngine.EventSystems;

public class UIObjectDraggableItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ItemOption linkedObject;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError($"[UIObjectDraggableItem] CanvasGroup missing on '{gameObject.name}'.");
        }

        //if (UIDragContext.Instance == null)
        //{
        //    Debug.LogError("[UIObjectDraggableItem] UIDragContext is missing in scene. Dragging will not work.");
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[UIObjectDraggableItem] Pointer down on {linkedObject.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UIDragContext.Instance == null)
        {
            Debug.LogError("[UIObjectDraggableItem] UIDragContext not found. Drag cancelled.");
            return;
        }

        if (linkedObject?.prefab == null)
        {
            Debug.LogError("[UIObjectDraggableItem] linkedObject or its prefab is null.");
            return;
        }

        Debug.Log($"[UIObjectDraggableItem] Begin drag {linkedObject.name}");

        DragPlacementHandler.Instance.BeginDrag(linkedObject.prefab);

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        DragPlacementHandler.Instance.OnDragCancelled += ResetVisualState;
    }

    public void OnDrag(PointerEventData eventData)
    {
        UIDragContext.Instance.UpdateContext(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[UIObjectDraggableItem] End drag {linkedObject.name}");

        ResetVisualState();
        TooltipManager.Instance.Hide();

        if (UIDragContext.Instance.IsPointerOverCancelZone)
        {
            DragPlacementHandler.Instance.CancelDrag();
            Debug.Log("[UIObjectDraggableItem] Drag cancelled (released over PanelRight).");
            return;
        }
        else
        {
            DragPlacementHandler.Instance.EndDrag(eventData);
        }

        DragPlacementHandler.Instance.OnDragCancelled -= ResetVisualState;
    }

    //private void HandleDragCancelled()
    //{
    //    ResetVisualState();
    //    TooltipManager.Instance.Hide();

    //    DragPlacementHandler.Instance.OnDragCancelled -= HandleDragCancelled;
    //}

    private void ResetVisualState()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}

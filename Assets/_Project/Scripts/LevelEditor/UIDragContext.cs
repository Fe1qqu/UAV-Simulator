using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragContext : MonoBehaviour
{
    public static UIDragContext Instance { get; private set; }

    [Tooltip("Tag bar that cancels dragging when hovered over")]
    public string cancelPanelTag = "PanelRight";

    public bool IsPointerOverCancelZone { get; private set; }

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

    public void UpdateContext(PointerEventData eventData)
    {
        var target = eventData.pointerEnter;
        bool overCancel = target != null && IsPointerOverTaggedParent(target, cancelPanelTag);

        if (overCancel && !IsPointerOverCancelZone)
        {
            /* Возможно не самый оптимизированный вариант */
            TooltipManager.Instance.Show("Cancel drag");
        }
        else if (!overCancel && IsPointerOverCancelZone)
        {
            TooltipManager.Instance.Hide();
        }

        IsPointerOverCancelZone = overCancel;
    }

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

    public void ResetContext()
    {
        IsPointerOverCancelZone = false;
        TooltipManager.Instance.Hide();
    }
}

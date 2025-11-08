using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragPlacementHandler : MonoBehaviour
{
    public static DragPlacementHandler Instance { get; private set; }

    public event System.Action OnDragCancelled;

    [Header("Placement settings")]
    public LayerMask placementLayerMask;
    public Transform levelRoot;

    [Header("Preview settings")]
    public Material previewMaterial;
    public float rayLength = 100000f;

    private GameObject prefabToPlace;
    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private bool isVisible;

    //private Material[] originalMaterials;

    public bool IsDragging => previewInstance != null;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[DragPlacementHandler] Duplicate instance detected. There should only be one in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void BeginDrag(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[DragPlacementHandler] BeginDrag called with null prefab.");
            return;
        }

        prefabToPlace = prefab;

        previewInstance = Instantiate(prefabToPlace);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
        isVisible = false;

        foreach (var renderer in previewRenderers)
        {
            renderer.material = previewMaterial;
            renderer.enabled = false;
        }

        // So that the preview doesn't interfere with the beam
        foreach (var сollider in previewInstance.GetComponentsInChildren<Collider>())
        {
            сollider.enabled = false;
        }
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (prefabToPlace == null)
        {
            Cleanup();
            return;
        }

        Vector2 screenPos = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, placementLayerMask))
        {
            Instantiate(prefabToPlace, hit.point, Quaternion.identity, levelRoot);
            Debug.Log($"[DragPlacementHandler] Placed '{prefabToPlace.name}' at {hit.point}");
        }
        else
        {
            Debug.Log($"[DragPlacementHandler] Cannot place '{prefabToPlace.name}' at pointer position (no valid hit).");
        }

        UIDragContext.Instance.ResetContext();
        Cleanup();
    }

    public void CancelDrag()
    {
        Debug.Log("[DragPlacementHandler] Drag cancelled.");
        OnDragCancelled?.Invoke();
        UIDragContext.Instance.ResetContext();
        Cleanup();
    }

    private void Cleanup()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        prefabToPlace = null;
        previewInstance = null;
        previewRenderers = null;
        //originalMaterials = null;
    }

    private void SetObjectPreviewVisible(bool visible)
    {
        //Debug.Log($"Пробуем менять видимость на {visible}");

        if (previewRenderers == null)
        {
            return;
        }

        if (visible == isVisible)
        {
            return;
        }

        //Debug.Log($"Поменяли видимость");

        foreach (var renderer in previewRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }

        isVisible = visible;
    }

    private void Update()
    {
        if (previewInstance == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (IsDragging && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("[DragPlacementHandler] Drag cancelled by user (RMB).");
            CancelDrag();
            TooltipManager.Instance.Hide();
            return;
        }

        if (UIDragContext.Instance != null && UIDragContext.Instance.IsPointerOverCancelZone)
        {
            SetObjectPreviewVisible(false);
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, placementLayerMask))
        {
            previewInstance.transform.position = hit.point;
            SetObjectPreviewVisible(true);
        }
        else
        {
            SetObjectPreviewVisible(false);
        }
    }
}

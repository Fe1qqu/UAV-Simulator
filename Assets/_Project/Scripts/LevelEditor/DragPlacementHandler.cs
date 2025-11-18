using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Handles placement drag workflow:
/// - Creates preview model
/// - Moves preview under mouse cursor via raycast
/// - Places object or cancels drag
/// - Notifies TooltipManager and UIDragContext
/// </summary>
public class DragPlacementHandler : MonoBehaviour
{
    public static DragPlacementHandler Instance { get; private set; }

    /// <summary>
    /// Fired when drag is cancelled.
    /// </summary>
    public event System.Action OnDragCancelled;

    [Header("Placement settings")]
    [Tooltip("Raycast mask used to find a valid placement surface.")]
    [SerializeField] private LayerMask placementLayerMask;

    [Tooltip("Parent under which placed objects will be instantiated.")]
    [SerializeField] private Transform levelRoot;

    [Header("Preview settings")]
    [Tooltip("Material applied to the preview instance during drag.")]
    [SerializeField] private Material previewMaterial;

    [Tooltip("Maximum ray distance used for placement checks.")]
    [SerializeField] private float rayLength = 100000f;

    private GameObject prefabToPlace;
    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private bool isVisible;

    /// <summary>
    /// True while a drag is in progress.
    /// </summary>
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

        if (levelRoot == null)
        {
            Debug.LogError("[DragPlacementHandler] LevelRoot is not assigned.");
        }
        
        if (previewMaterial == null)
        {
            Debug.LogError("[DragPlacementHandler] PreviewMaterial is not assigned.");
        }
    }

    /// <summary>
    /// Starts placement drag by creating a hidden preview instance.
    /// </summary>
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

        // Disable colliders to avoid self-intersection
        foreach (var ñollider in previewInstance.GetComponentsInChildren<Collider>())
        {
            ñollider.enabled = false;
        }

        //Debug.Log($"[DragPlacementHandler] Drag started for '{prefabToPlace.name}'.");
    }

    /// <summary>
    /// Confirms placement, performs raycast and spawns final object.
    /// </summary>
    public void EndDrag(PointerEventData eventData)
    {
        if (prefabToPlace == null)
        {
            Debug.LogWarning("[DragPlacementHandler] EndDrag called but prefabToPlace is null.");
            Cleanup();
            return;
        }

        Vector2 screenPos = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, placementLayerMask))
        {
            Instantiate(prefabToPlace, hit.point, Quaternion.identity, levelRoot);
            Debug.Log($"[DragPlacementHandler] Placed '{prefabToPlace.name}' at world pos {hit.point}");
        }
        else
        {
            Debug.Log($"[DragPlacementHandler] Failed to place '{prefabToPlace.name}' — no valid surface at pointer.");
        }

        UIDragContext.Instance.ResetContext();
        Cleanup();
    }

    /// <summary>
    /// Cancels drag, hides preview, resets states, notifies listeners.
    /// </summary>
    public void CancelDrag()
    {
        Debug.Log("[DragPlacementHandler] Drag cancelled.");

        OnDragCancelled?.Invoke();
        UIDragContext.Instance.ResetContext();
        Cleanup();
    }

    /// <summary>
    /// Destroys preview, clears drag state.
    /// </summary>
    private void Cleanup()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        prefabToPlace = null;
        previewInstance = null;
        previewRenderers = null;
    }

    /// <summary>
    /// Controls renderer visibility to avoid redundant enabling/disabling.
    /// </summary>
    private void SetObjectPreviewVisible(bool visible)
    {
        if (previewRenderers == null)
        {
            return;
        }

        if (visible == isVisible)
        {
            return;
        }

        isVisible = visible;

        foreach (var renderer in previewRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
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

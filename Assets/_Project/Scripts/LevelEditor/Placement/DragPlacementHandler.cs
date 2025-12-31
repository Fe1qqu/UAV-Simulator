using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Handles placement drag workflow:
/// - Creates preview model
/// - Moves preview under mouse cursor via raycast
/// - Places object or cancels drag
/// - Notifies TooltipManager and UIDragContext
/// </summary>
public class DragPlacementHandler : MonoBehaviour, IBackHandler
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
    [Tooltip("Default material applied to the preview instance during drag.")]
    [SerializeField] private Material defaultPreviewMaterial;

    [Tooltip("Maximum ray distance used for placement checks.")]
    [SerializeField] private float rayLength = 100000f;

    private PlaceableObjectData placeableData;
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
        
        if (defaultPreviewMaterial == null)
        {
            Debug.LogError("[DragPlacementHandler] DefaultPreviewMaterial is not assigned.");
        }
    }

    /// <summary>
    /// Starts placement drag by creating a hidden preview instance.
    /// </summary>
    public void BeginDrag(PlaceableObjectData data)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("[DragPlacementHandler] BeginDrag called with invalid PlaceableObjectData.");
            return;
        }

        placeableData = data;

        previewInstance = Instantiate(data.prefab);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
        isVisible = false;

        foreach (Renderer renderer in previewRenderers)
        {
            switch (data.previewMaterialMode)
            {
                case PreviewMaterialMode.UseDefault:
                    renderer.material = defaultPreviewMaterial;
                    break;

                case PreviewMaterialMode.Override:
                    if (data.previewMaterialOverride != null)
                    {
                        renderer.material = data.previewMaterialOverride;
                    }
                    else
                    {
                        Debug.LogError("[DragPlacementHandler] PreviewMaterialOverride is not assigned.");
                    }
                    break;

                case PreviewMaterialMode.None:
                    // Do nothing - leave the original prefab material
                    break;
            }

            renderer.enabled = false;
        }

        // Disable colliders to avoid self-intersection
        foreach (Collider ñollider in previewInstance.GetComponentsInChildren<Collider>())
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
        if (placeableData == null)
        {
            //Debug.LogWarning("[DragPlacementHandler] EndDrag called but placeableData is null.");
            Cleanup();
            return;
        }
        
        Vector2 screenPos = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, placementLayerMask))
        {
            GameObject instance = Instantiate(placeableData.prefab, hit.point, Quaternion.identity, levelRoot);
            if (!instance.TryGetComponent<LevelObject>(out var levelObject))
            {
                Debug.LogError($"[DragPlacementHandler] {instance.name} has no LevelObject component.");
            }
            levelObject.Initialize(placeableData.objectId);

            // Create an undo/redo action
            RTG.PostObjectSpawnAction spawnAction = new RTG.PostObjectSpawnAction(new List<GameObject> { instance });
            spawnAction.Execute();

            // Automatically select the installed object
            if (instance.TryGetComponent<SelectableObject>(out var selectable))
            {
                SelectionManager.Instance.Select(selectable);
            }
            else
            {
                Debug.Log("[DragPlacementHandler] Failed to automatically select the installed object.");
            }

            Debug.Log($"[DragPlacementHandler] Placed '{placeableData.prefab.name}' at world pos {hit.point}");
        }
        else
        {
            Debug.Log($"[DragPlacementHandler] Failed to place '{placeableData.prefab.name}' — no valid surface at pointer.");
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

        placeableData = null;
        previewInstance = null;
        previewRenderers = null;

        TooltipManager.Instance.Hide();
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

        foreach (Renderer renderer in previewRenderers)
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

    public bool OnBack()
    {
        return false;
        // TO DO: Cancel drag
        //if (!IsDragging)
        //{
        //    return false;
        //}
        //CancelDrag();
        //return true;
    }
}

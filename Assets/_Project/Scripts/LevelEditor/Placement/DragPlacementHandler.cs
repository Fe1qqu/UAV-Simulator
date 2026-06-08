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

    private PlaceableObjectDefinition currentPlaceableObject;
    private PreviewObject previewObject;
    private GameObject previewInstance;
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
    public void BeginDrag(PlaceableObjectDefinition placeableObject)
    {
        if (placeableObject == null || placeableObject.prefab == null)
        {
            Debug.LogError("[DragPlacementHandler] BeginDrag called with invalid PlaceableObject.");
            return;
        }

        currentPlaceableObject = placeableObject;

        previewInstance = Instantiate(placeableObject.prefab);

        if (!previewInstance.TryGetComponent(out previewObject))
        {
            Debug.LogError($"[DragPlacementHandler] {previewInstance.name} has no PreviewObject component.");

            Destroy(previewInstance);

            previewInstance = null;
            previewObject = null;
            currentPlaceableObject = null;

            return;
        }

        previewObject.EnablePreviewMode(placeableObject.previewMaterialMode, defaultPreviewMaterial, placeableObject.previewMaterialOverride);

        isVisible = false;

        //Debug.Log($"[DragPlacementHandler] Drag started for '{prefabToPlace.name}'.");
    }

    /// <summary>
    /// Confirms placement, performs raycast and spawns final object.
    /// </summary>
    public void EndDrag(PointerEventData eventData)
    {
        if (currentPlaceableObject == null)
        {
            Debug.LogWarning("[DragPlacementHandler] CurrentPlaceableObject is null.");
            CancelDrag();
            return;
        }
        
        Vector2 screenPos = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, placementLayerMask))
        {
            previewInstance.transform.position = hit.point;
            previewInstance.transform.rotation = Quaternion.identity;
            previewInstance.transform.SetParent(levelRoot);

            previewObject.DisablePreviewMode();

            GameObject instance = previewInstance;

            if (!instance.TryGetComponent<LevelObject>(out var levelObject))
            {
                Debug.LogError($"[DragPlacementHandler] {instance.name} has no LevelObject component.");
            }
            else
            {
                levelObject.Initialize(currentPlaceableObject);
            }

            // Create an undo/redo action
            RTG.PostObjectSpawnAction spawnAction = new(new List<GameObject> { instance });
            spawnAction.Execute();

            // Automatically select the installed object
            if (instance.TryGetComponent<SelectableObject>(out var selectableObject))
            {
                SelectionManager.Instance.Select(selectableObject);
            }
            else
            {
                Debug.Log("[DragPlacementHandler] Failed to automatically select the installed object.");
            }
            
            Debug.Log($"[DragPlacementHandler] Placed '{currentPlaceableObject.prefab.name}' at world pos {hit.point}.");
        }
        else
        {
            Debug.Log($"[DragPlacementHandler] Failed to place '{currentPlaceableObject.prefab.name}' � no valid surface at pointer.");
        }

        UIDragContext.Instance.ResetContext();
        FinishPlacement();
    }

    /// <summary>
    /// Cancels drag, hides preview, resets states, notifies listeners.
    /// </summary>
    public void CancelDrag()
    {
        Debug.Log("[DragPlacementHandler] Drag cancelled.");

        OnDragCancelled?.Invoke();
        UIDragContext.Instance.ResetContext();

        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        FinishPlacement();
    }

    private void FinishPlacement()
    {
        currentPlaceableObject = null;
        previewInstance = null;
        previewObject = null;

        TooltipManager.Instance.Hide();
    }

    /// <summary>
    /// Controls renderer visibility to avoid redundant enabling/disabling.
    /// </summary>
    private void SetObjectPreviewVisible(bool visible)
    {
        if (previewObject == null)
        {
            return;
        }

        if (visible == isVisible)
        {
            return;
        }

        isVisible = visible;
        previewObject.SetVisible(visible);
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

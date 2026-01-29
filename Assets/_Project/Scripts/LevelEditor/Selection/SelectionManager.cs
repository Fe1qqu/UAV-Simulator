using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public event System.Action<SelectableObject> OnSelectionChanged;

    [Tooltip("Mask for selectable objects.")]
    [SerializeField] private LayerMask selectableMask = 0;

    [Tooltip("Maximum ray distance used for selection checks.")]
    [SerializeField] private float rayLength = 100000f;

    // Currently selected object
    public SelectableObject CurrentSelectedObject { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[SelectionManager] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // Block the selector's operation if the gizmo is under the cursor or is being dragged
        if (RTG.RTGizmosEngine.Get.IsAnyGizmoHovered || RTG.RTGizmosEngine.Get.DraggedGizmo != null)
        {
            return; 
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            TrySelectObject();
        }
    }

    private void TrySelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, selectableMask))
        {
            SelectableObject selectableObject = hit.collider.GetComponentInParent<SelectableObject>();
            if (selectableObject != null)
            {
                SelectObject(selectableObject);
            }
            return;
        }

        DeselectCurrentObject();
    }

    public void SelectObject(SelectableObject selectableObject)
    {
        if (CurrentSelectedObject == selectableObject)
        {
            return;
        }

        DeselectCurrentObject();

        CurrentSelectedObject = selectableObject;
        CurrentSelectedObject.Select();

        OnSelectionChanged?.Invoke(CurrentSelectedObject);
    }

    public void DeselectCurrentObject()
    {
        if (CurrentSelectedObject == null)
        {
            return;
        }

        CurrentSelectedObject.Deselect();
        CurrentSelectedObject = null;

        OnSelectionChanged?.Invoke(null);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    /// <summary>
    /// Fired when selection changes. Null means deselection.
    /// </summary>
    public event System.Action<ISelectable> OnSelectionChanged;

    [Header("Raycast")]
    [Tooltip("Mask for selectable objects.")]
    [SerializeField] private LayerMask selectableMask = 0;

    [Tooltip("Maximum ray distance used for selection checks.")]
    [SerializeField] private float rayLength = 100000f;

    // Currently selectable
    private ISelectable current;
    public ISelectable Current => current;

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
            TrySelect();
        }
    }

    private void TrySelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, selectableMask))
        {
            ISelectable selectable = hit.collider.GetComponentInParent<ISelectable>();
            if (selectable != null)
            {
                Select(selectable);
                return;
            }
        }

        Deselect();
    }

    public void Select(ISelectable selectable)
    {
        if (current == selectable)
        {
            return;
        }

        current?.OnDeselected();

        current = selectable;

        current?.OnSelected();

        OnSelectionChanged?.Invoke(current);
    }

    public void Deselect()
    {
        Select(null);
    }
}

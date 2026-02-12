using UnityEngine;
using RTG;

public class TransformHandleController : MonoBehaviour
{
    public static TransformHandleController Instance { get; private set; }

    private ObjectTransformGizmo gizmo;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[TransformHandleController] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (SelectionManager.Instance == null)
        {
            Debug.Log("[TransformHandleController] SelectionManager instance is null.");
            return; 
        }

        SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;

        gizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();
        gizmo.Gizmo.SetEnabled(false);
    }

    private void OnDestroy()
    {
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
        }
    }

    private void HandleSelectionChanged(ISelectable selectable)
    {
        // Nothing selected → hide gizmo
        if (selectable == null)
        {
            gizmo.Gizmo.SetEnabled(false);
            return;
        }

        if (selectable is not Component component)
        {
            gizmo.Gizmo.SetEnabled(false);
            return;
        }

        if (!component.TryGetComponent<TransformableObject>(out var _))
        {
            gizmo.Gizmo.SetEnabled(false);
            return;
        }

        gizmo.SetTargetObject(component.gameObject);
        gizmo.Gizmo.SetEnabled(true);
    }
}

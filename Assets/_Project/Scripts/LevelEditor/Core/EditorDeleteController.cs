using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

public class EditorDeleteController : MonoBehaviour
{
    [SerializeField] private EditorInput editorInput;

    private void Awake()
    {
        if (editorInput == null)
        {
            Debug.LogError("[EditorDeleteController] EditorInput is not assigned.");
        }
    }
    private void OnEnable()
    {
        editorInput.DeleteSelected += OnDeleteSelected;
    }

    private void OnDisable()
    {
        editorInput.DeleteSelected -= OnDeleteSelected;
    }

    private void OnDeleteSelected()
    {
        //if (DragPlacementHandler.Instance != null && DragPlacementHandler.Instance.IsDragging)
        //{
        //    Debug.Log("[EditorDeleteController] Delete ignored: drag in progress.");
        //    return;
        //}

        SelectionManager selectionManager = SelectionManager.Instance;
        if (selectionManager == null)
        {
            Debug.Log("[EditorDeleteController] SelectionManager is null.");
            return;
        }

        SelectableObject selectedObject = selectionManager.Current;
        if (selectedObject == null)
        {
            Debug.LogWarning("[EditorDeleteController] SelectedObject is null.");
            return;
        }

        GameObject target = selectedObject.gameObject;

        selectionManager.DeselectCurrent();
        target.SetActive(false);

        // Create an undo/redo delete action
        PostObjectDeleteAction deleteAction = new PostObjectDeleteAction(target);
        deleteAction.Execute();

        Debug.Log($"[EditorDeleteController] Deleted object '{gameObject.name}'.");
    }
}

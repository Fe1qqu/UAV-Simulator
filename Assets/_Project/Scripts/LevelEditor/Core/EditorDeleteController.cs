using UnityEngine;

public class EditorDeleteController : MonoBehaviour
{
    [SerializeField] private EditorInput editorInput;
    [SerializeField] private SelectionManager selectionManager;

    private void Awake()
    {
        if (editorInput == null)
        {
            Debug.LogError("[EditorDeleteController] EditorInput is not assigned.");
        }

        if (selectionManager == null)
        {
            Debug.LogError("[EditorDeleteController] SelectionManager is not assigned.");
        }
    }

    private void OnEnable()
    {
        editorInput.Delete += DeleteSelectedObject;
    }

    private void OnDisable()
    {
        editorInput.Delete -= DeleteSelectedObject;
    }

    public void DeleteSelectedObject()
    {
        ISelectable selected = selectionManager.Current;
        if (selected == null)
        {
            Debug.LogError("[EditorDeleteController] Nothing selected.");
            return;
        }

        if (selected is not Component component || !component.TryGetComponent<LevelObject>(out var levelObject))
        {
            Debug.LogError("[EditorDeleteController] Selected object has no LevelObject component.");
            return;
        }

        DeleteInternal(levelObject, selected);
    }

    public void DeleteObject(LevelObject levelObject)
    {
        if (levelObject == null)
        {
            Debug.LogError("[EditorDeleteController] LevelObject is null.");
            return;
        }

        if (!levelObject.TryGetComponent(out SelectableObject selectableObject))
        {
            Debug.LogError("[EditorDeleteController] LevelObject has no SelectableObject ñomponent.");
            return;
        }

        DeleteInternal(levelObject, selectableObject);
    }

    private void DeleteInternal(LevelObject levelObject, ISelectable selectable)
    {
        bool wasSelected = selectionManager.Current == selectable;

        if (wasSelected)
        {
            selectionManager.Deselect();
        }

        levelObject.SoftDelete();

        // Create an undo/redo delete action
        PostLevelObjectDeleteAction deleteAction = new(levelObject, wasSelected);
        deleteAction.Execute();

        Debug.Log($"[EditorDeleteController] Deleted object '{levelObject.name}'.");
    }
}

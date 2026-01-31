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
        SelectableObject selectedObject = selectionManager.CurrentSelectedObject;
        if (selectedObject == null)
        {
            Debug.LogError("[EditorDeleteController] SelectedObject is null.");
            return;
        }

        if (!selectedObject.TryGetComponent<LevelObject>(out var levelObject))
        {
            Debug.LogError("[EditorDeleteController] Selected object has no LevelObject component.");
            return;
        }

        DeleteObjectInternal(levelObject, selectedObject);
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

        DeleteObjectInternal(levelObject, selectableObject);
    }

    private void DeleteObjectInternal(LevelObject levelObject, SelectableObject selectableObject)
    {
        bool wasSelected = selectionManager.CurrentSelectedObject == selectableObject;

        if (wasSelected)
        {
            selectionManager.DeselectCurrentObject();
        }

        levelObject.SoftDelete();

        // Create an undo/redo delete action
        PostLevelObjectDeleteAction deleteAction = new PostLevelObjectDeleteAction(levelObject, wasSelected);
        deleteAction.Execute();

        Debug.Log($"[EditorDeleteController] Deleted object '{levelObject.name}'.");
    }
}

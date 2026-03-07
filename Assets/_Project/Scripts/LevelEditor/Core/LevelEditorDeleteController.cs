using UnityEngine;

public class LevelEditorDeleteController : MonoBehaviour
{
    [SerializeField] private LevelEditorInput levelEditorInput;
    [SerializeField] private SelectionManager selectionManager;

    private void Awake()
    {
        if (levelEditorInput == null)
        {
            Debug.LogError("[LevelEditorDeleteController] LevelEditorInput is not assigned.");
        }

        if (selectionManager == null)
        {
            Debug.LogError("[LevelEditorDeleteController] SelectionManager is not assigned.");
        }
    }

    private void OnEnable()
    {
        levelEditorInput.DeleteRequested += DeleteSelectedObject;
    }

    private void OnDisable()
    {
        levelEditorInput.DeleteRequested -= DeleteSelectedObject;
    }

    public void DeleteSelectedObject()
    {
        ISelectable selected = selectionManager.Current;
        if (selected == null)
        {
            Debug.LogError("[LevelEditorDeleteController] Nothing selected.");
            return;
        }

        if (selected is not Component component || !component.TryGetComponent<LevelObject>(out var levelObject))
        {
            Debug.LogError("[LevelEditorDeleteController] Selected object has no LevelObject component.");
            return;
        }

        DeleteInternal(levelObject, selected);
    }

    public void DeleteObject(LevelObject levelObject)
    {
        if (levelObject == null)
        {
            Debug.LogError("[LevelEditorDeleteController] LevelObject is null.");
            return;
        }

        if (!levelObject.TryGetComponent(out SelectableObject selectableObject))
        {
            Debug.LogError("[LevelEditorDeleteController] LevelObject has no SelectableObject ñomponent.");
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

        Debug.Log($"[LevelEditorDeleteController] Deleted object '{levelObject.name}'.");
    }
}

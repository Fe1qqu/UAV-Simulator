using RTG;

public class PostLevelObjectDeleteAction : IUndoRedoAction
{
    private readonly LevelObject levelObject;

    private bool wasSelected;

    public PostLevelObjectDeleteAction(LevelObject levelObject, bool wasSelected)
    {
        this.levelObject = levelObject;
        this.wasSelected = wasSelected;
    }

    public void Execute()
    {
        RTUndoRedo.Get.RecordAction(this);
    }

    public void Undo()
    {
        if (levelObject == null)
        {
            return;
        }

        levelObject.Restore();

        if (wasSelected)
        {
            SelectionManager selectionManager = SelectionManager.Instance;
            if (selectionManager != null)
            {
                if (levelObject.TryGetComponent<SelectableObject>(out var selectableObject))
                {
                    selectionManager.SelectObject(selectableObject);
                }
            }
        }
    }

    public void Redo()
    {
        if (levelObject == null)
        {
            return;
        }

        SelectionManager selectionManager = SelectionManager.Instance;
        if (selectionManager != null)
        {
            SelectableObject selectableObject = levelObject.GetComponent<SelectableObject>();
            if (selectableObject != null && selectionManager.CurrentSelectedObject == selectableObject)
            {
                selectionManager.DeselectCurrentObject();
            }
        }

        levelObject.SoftDelete();
    }

    public void OnRemovedFromUndoRedoStack()
    {
        // Intentionally empty: soft delete only
    }
}

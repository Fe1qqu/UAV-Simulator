using RTG;

public class PostLevelObjectDeleteAction : IUndoRedoAction
{
    private readonly LevelObject levelObject;

    public PostLevelObjectDeleteAction(LevelObject levelObject)
    {
        this.levelObject = levelObject;
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

        SelectionManager selectionManager = SelectionManager.Instance;
        if (selectionManager == null)
        {
            return;
        }

        SelectableObject selectableObject = levelObject.GetComponent<SelectableObject>();
        if (selectableObject == null)
        {
            return;
        }

        selectionManager.SelectObject(selectableObject);
    }

    public void Redo()
    {
        if (levelObject == null)
        {
            return;
        }

        levelObject.SoftDelete();
    }

    public void OnRemovedFromUndoRedoStack()
    {
    }
}

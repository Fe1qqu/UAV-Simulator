using UnityEngine;
using RTG;

public enum TransformType
{
    Position,
    Rotation,
    Scale
}

public class TransformChangeAction : IUndoRedoAction
{
    private LevelObject levelObject;
    private Vector3 oldValue;
    private Vector3 newValue;
    private TransformType type;

    public TransformChangeAction(LevelObject levelObject, Vector3 oldValue, Vector3 newValue, TransformType type)
    {
        this.levelObject = levelObject;
        this.oldValue = oldValue;
        this.newValue = newValue;
        this.type = type;
    }

    public void Execute() => RTUndoRedo.Get.RecordAction(this);

    public void Undo()
    {
        if (levelObject == null)
        {
            return;
        }

        Apply(oldValue);
    }

    public void Redo()
    {
        if (levelObject == null)
        {
            return;
        }

        Apply(newValue);
    }

    private void Apply(Vector3 value)
    {
        switch (type)
        {
            case TransformType.Position:
                levelObject.SetPosition(value);
                break;

            case TransformType.Rotation:
                levelObject.SetRotation(value);
                break;

            case TransformType.Scale:
                levelObject.SetScale(value);
                break;
        }
    }

    public void OnRemovedFromUndoRedoStack() { }
}

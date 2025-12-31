using UnityEngine;
using RTG;

public class PostObjectDeleteAction : IUndoRedoAction
{
    private GameObject _target;

    public PostObjectDeleteAction(GameObject target)
    {
        _target = target;
    }

    public void Execute()
    {
        RTUndoRedo.Get.RecordAction(this);
    }

    public void Undo()
    {
        if (_target != null)
        {
            _target.SetActive(true);
        }
    }

    public void Redo()
    {
        if (_target != null)
        {
            _target.SetActive(false);
        }
    }

    public void OnRemovedFromUndoRedoStack()
    {
        if (_target != null)
        {
            GameObject.Destroy(_target);
        }
    }
}

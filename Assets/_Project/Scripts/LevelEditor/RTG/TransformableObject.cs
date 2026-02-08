using UnityEngine;
using RTG;

[RequireComponent(typeof(LevelObject))]
public class TransformableObject : MonoBehaviour, IRTTransformGizmoListener
{
    private LevelObject levelObject;

    private Vector3 _preTransformPosition;
    private Vector3 _preTransformRotation;
    private Vector3 _preTransformScale;

    private Gizmo activeGizmo;

    private void Awake()
    {
        levelObject = GetComponent<LevelObject>();
        if (levelObject == null)
        {
            Debug.LogError("[TransformableObject] LevelObject is missing!");
        }
    }

    private void OnEnable()
    {
        if (levelObject != null)
        {
            levelObject.TransformChanged += OnLevelObjectTransformChanged;
        }
    }

    private void OnDisable()
    {
        if (levelObject != null)
        {
            levelObject.TransformChanged -= OnLevelObjectTransformChanged;
        }
    }

    // Doesn't work perfectly
    private void OnLevelObjectTransformChanged(LevelObject _)
    {
        if (activeGizmo == null)
        {
            return;
        }

        activeGizmo.Transform.Position3D = levelObject.transform.position;
        activeGizmo.ObjectTransformGizmo.RefreshPosition();
    }

    public bool OnCanBeTransformed(Gizmo gizmo)
    {
        activeGizmo = gizmo;
        return levelObject != null && levelObject.IsAlive;
    }

    public void OnTransformStarted(Gizmo gizmo)
    {
        if (levelObject == null)
        {
            return;
        }

        // сохраняем старые значения для Undo
        _preTransformPosition = levelObject.transform.position;
        _preTransformRotation = levelObject.transform.rotation.eulerAngles;
        _preTransformScale = levelObject.transform.localScale;
    }

    public void OnTransformed(Gizmo gizmo)
    {
        if (levelObject == null)
        {
            return;
        }    

        Vector3 newPosition = levelObject.transform.position;
        Vector3 newRotation = levelObject.transform.rotation.eulerAngles;
        Vector3 newScale = levelObject.transform.localScale;

        levelObject.SetPosition(newPosition, createUndo: true);
        levelObject.SetRotation(newRotation, createUndo: true);
        levelObject.SetScale(newScale, createUndo: true);
    }

    public void OnTransformCompleted(Gizmo gizmo)
    {
        if (levelObject == null)
        {
            return;
        }

        Vector3 finalPosition = levelObject.transform.position;
        Vector3 finalRotation = levelObject.transform.rotation.eulerAngles;
        Vector3 finalScale = levelObject.transform.localScale;

        if (_preTransformPosition != finalPosition)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(levelObject, _preTransformPosition, finalPosition, TransformType.Position));
        }

        if (_preTransformRotation != finalRotation)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(levelObject, _preTransformRotation, finalRotation, TransformType.Rotation));
        }

        if (_preTransformScale != finalScale)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(levelObject, _preTransformScale, finalScale, TransformType.Scale));
        }
    }
}

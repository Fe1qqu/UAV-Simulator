using UnityEngine;
using RTG;

public class TransformableObject : MonoBehaviour, IRTTransformGizmoListener
{
    public bool OnCanBeTransformed(Gizmo gizmo)
    {
        // Can be transformed
        return true;
    }

    public void OnTransformed(Gizmo gizmo)
    {
    }
}

using UnityEngine;
using System;

public class Checkpoint : LevelObject, ITriggerReceiver
{
    public event Action<Checkpoint> OnEntered;

    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Material unpassedMaterial;
    [SerializeField] private Material passedMaterial;

    private bool isPassed;

    private void Awake()
    {
        if (meshRenderer == null)
        {
            Debug.LogError("[Checkpoint] MeshRenderer is not assigned.");
        }

        if (unpassedMaterial == null)
        {
            Debug.LogError("[Checkpoint] UnpassedMaterial is not assigned.");
        }

        if (passedMaterial == null)
        {
            Debug.LogError("[Checkpoint] PassedMaterial is not assigned.");
        }

        ResetState();
    }

    public void MarkPassed()
    {
        if (isPassed)
        {
            return;
        }

        isPassed = true;
        meshRenderer.sharedMaterial = passedMaterial;
    }

    public void ResetState()
    {
        isPassed = false;
        meshRenderer.sharedMaterial = unpassedMaterial;
    }

    public void OnTriggerEntered(Collider collider)
    {
        if (isPassed)
        {
            return;
        }

        var drone = collider.GetComponentInParent<IDroneActor>();
        if (drone == null)
        {
            Debug.LogWarning(
                $"[Checkpoint] Non-drone object entered trigger. Checkpoint: {name}, " +
                $"Object: {collider.name}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}."
            );

            return;
        }

        OnEntered?.Invoke(this);
    }

    public void OnTriggerExited(Collider collider) { }
}

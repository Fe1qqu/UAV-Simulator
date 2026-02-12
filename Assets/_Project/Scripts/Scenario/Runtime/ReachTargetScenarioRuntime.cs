using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReachTargetScenarioRuntime : ScenarioRuntimeBase
{
    private List<Checkpoint> checkpoints;
    private TriggerArea targetArea;

    private int currentCheckpointIndex;

    private bool allCheckpointsPassed;
    private bool droneInsideTarget;

    public override void StartScenario()
    {
        checkpoints = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .OrderBy(checkpoint => checkpoint.GetInt("index"))
            .ToList();

        targetArea = levelObjectRegistry.FindFirstAlive<TriggerArea>();
        if (targetArea == null)
        {
            Debug.LogError("[ReachTargetScenarioRuntime] TriggerArea not found.");
        }

        currentCheckpointIndex = 0;
        allCheckpointsPassed = false;
        droneInsideTarget = false;

        Subscribe();
    }

    private void Subscribe()
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.OnEntered += OnCheckpointEntered;
        }

        targetArea.ObjectEntered += OnTargetAreaObjectEntered;
        targetArea.ObjectExited += OnTargetAreaObjectExited;
    }

    public override void DisposeScenario()
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.OnEntered -= OnCheckpointEntered;
        }

        targetArea.ObjectEntered -= OnTargetAreaObjectEntered;
        targetArea.ObjectExited -= OnTargetAreaObjectExited;

        checkpoints.Clear();
        targetArea = null;
    }

    private void OnCheckpointEntered(Checkpoint checkpoint)
    {
        if (checkpoint != checkpoints[currentCheckpointIndex])
        {
            return;
        }

        checkpoint.MarkPassed();

        Debug.Log($"[ReachTargetScenarioRuntime] Checkpoint {currentCheckpointIndex} passed.");
        currentCheckpointIndex++;

        if (currentCheckpointIndex >= checkpoints.Count)
        {
            allCheckpointsPassed = true;
        }
    }

    private void OnTargetAreaObjectEntered(Collider collider)
    {
        var drone = collider.GetComponentInParent<IDroneActor>();
        if ((Object)drone != droneController)
        {
            return;
        }

        droneInsideTarget = true;

        if (!allCheckpointsPassed)
        {
            Debug.Log("[ReachTargetScenarioRuntime] Target reached before checkpoints.");
        }
    }

    private void OnTargetAreaObjectExited(Collider collider)
    {
        var drone = collider.GetComponentInParent<IDroneActor>();
        if ((Object)drone != droneController)
        {
            return;
        }

        droneInsideTarget = false;
    }

    public override void TickScenario()
    {
        if (!allCheckpointsPassed)
        {
            return;
        }

        if (!droneInsideTarget)
        {
            return;
        }

        //if (!droneController.IsLanded) // Or/And Disarm
        //{
        //    return;
        //}

        if (!DroneIsLanded())
        {
            return;
        }

        CompleteScenario();
    }

    public override void ResetScenario()
    {
        currentCheckpointIndex = 0;
        allCheckpointsPassed = false;
        droneInsideTarget = false;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.ResetState();
        }
    }

    private bool DroneIsLanded()
    {
        if (droneController == null)
        {
            return false;
        }

        if (!droneController.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return false;
        }

        const float maxVerticalSpeed = 0.5f;
        const float maxTiltAngle = 10f;

        bool verticalOk = Mathf.Abs(rigidbody.linearVelocity.y) < maxVerticalSpeed;
        bool tiltOk = Vector3.Angle(droneController.transform.up, Vector3.up) < maxTiltAngle;

        return verticalOk && tiltOk;
    }
}

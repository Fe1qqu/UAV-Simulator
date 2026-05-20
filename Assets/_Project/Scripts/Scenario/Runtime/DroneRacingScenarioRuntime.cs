using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Drone Racing")]
public class DroneRacingScenarioRuntime : ScenarioRuntimeBase
{
    private List<Checkpoint> checkpoints;
    private TargetArea targetArea;

    private int currentCheckpointIndex;

    private bool allCheckpointsPassed;
    private bool droneInsideTarget;

    protected override void StartScenarioInternal()
    {
        checkpoints = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .OrderBy(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, 0))
            .ToList();

        targetArea = levelObjectRegistry.FindFirstAlive<TargetArea>();
        if (targetArea == null)
        {
            Debug.LogError("[DroneRacingScenarioRuntime] TargetArea not found.");
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

    private void Unsubscribe()
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.OnEntered -= OnCheckpointEntered;
        }

        targetArea.ObjectEntered -= OnTargetAreaObjectEntered;
        targetArea.ObjectExited -= OnTargetAreaObjectExited;
    }

    protected override void TickScenarioInternal()
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

    protected override void ResetScenarioInternal()
    {
        currentCheckpointIndex = 0;
        allCheckpointsPassed = false;
        droneInsideTarget = false;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.ResetState();
        }
    }

    protected override void DisposeScenarioInternal()
    {
        Unsubscribe();
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

        Debug.Log($"[DroneRacingScenarioRuntime] Checkpoint {currentCheckpointIndex} passed.");
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
            Debug.Log("[DroneRacingScenarioRuntime] Target reached before checkpoints.");
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

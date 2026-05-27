using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Drone Racing")]
public class DroneRacingScenarioRuntime : ScenarioRuntimeBase
{
    private List<Checkpoint> checkpoints;
    private int currentCheckpointIndex;

    protected override void StartScenarioInternal()
    {
        checkpoints = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .OrderBy(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, 0))
            .ToList();

        if (checkpoints.Count == 0)
        {
            Debug.LogError("[DroneRacingScenarioRuntime] No checkpoints found.");
            return;
        }

        currentCheckpointIndex = 0;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.OnEntered += OnCheckpointEntered;
        }
    }

    protected override void ResetScenarioInternal()
    {
        currentCheckpointIndex = 0;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.ResetState();
        }
    }

    protected override void DisposeScenarioInternal()
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.OnEntered -= OnCheckpointEntered;
        }

        checkpoints.Clear();
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
            ConcludeGameplay();
            CompleteScenario();
        }
    }
}

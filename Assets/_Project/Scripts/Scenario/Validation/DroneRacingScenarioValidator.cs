using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Scenario Validators/Drone Racing")]
public class DroneRacingScenarioValidator : ScenarioSpecificValidator
{
    public override void Validate(LevelObjectRegistry levelObjectRegistry, ScenarioValidationResult result)
    {
        var checkpoints = levelObjectRegistry.EnumerateAlive<Checkpoint>();

        if (!CheckpointSequenceService.TryGetValidSequence(checkpoints, out var _))
        {
            result.Add(new ValidationIssue(ScenarioValidationErrorType.LevelInvalid, "validation_checkpoint_sequence"));
        }
    }
}

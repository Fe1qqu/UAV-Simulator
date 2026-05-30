using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Game Data/Scenario Validators/Drone Racing")]
public class DroneRacingScenarioValidator : ScenarioSpecificValidator
{
    public override void Validate(LevelObjectRegistry levelObjectRegistry, ScenarioValidationResult result)
    {
        var indices = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .Select(x => x.Get(LevelPropertyKeys.Index, -1))
            .OrderBy(x => x)
            .ToArray();

        bool valid = indices.SequenceEqual(Enumerable.Range(0, indices.Length));

        if (!valid)
        {
            result.Add(new ValidationIssue(ScenarioValidationErrorType.LevelInvalid, "validation_checkpoint_sequence"));
        }
    }
}

using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Game Data/Scenario Validators/Drone Racing")]
public class DroneRacingScenarioValidator : ScenarioSpecificValidator
{
    public override ScenarioValidationResult Validate(LevelObjectRegistry levelObjectRegistry)
    {
        var indices = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .Select(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, -1))
            .OrderBy(i => i);

        if (!indices.SequenceEqual(Enumerable.Range(0, indices.Count())))
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, "Checkpoint indices must form a continuous sequence 0..n-1");
        }

        return ScenarioValidationResult.Ok();
    }
}

using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Game Data/Scenario Validators/Reach Target")]
public class ReachTargetScenarioValidator : ScenarioSpecificValidator
{
    public override ScenarioValidationResult Validate(LevelObjectRegistry levelObjectRegistry)
    {
        var checkpoints = levelObjectRegistry
            .EnumerateAlive<Checkpoint>()
            .ToList();

        if (checkpoints.Count == 0)
        {
            return ScenarioValidationResult.Ok();
        }

        var indices = checkpoints
            .Select(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, -1))
            .OrderBy(i => i)
            .ToList();

        for (int i = 0; i < indices.Count; i++)
        {
            if (indices[i] != i)
            {
                return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, "Checkpoint indices must form a continuous sequence 0..n-1");
            }
        }

        return ScenarioValidationResult.Ok();
    }
}

using UnityEngine;

public abstract class ScenarioSpecificValidator : ScriptableObject
{
    public abstract ScenarioValidationResult Validate(LevelObjectRegistry levelObjectRegistry);
}

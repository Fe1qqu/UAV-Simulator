using UnityEngine;

public abstract class ScenarioSpecificValidator : ScriptableObject
{
    public abstract void Validate(LevelObjectRegistry levelObjectRegistry, ScenarioValidationResult result);
}

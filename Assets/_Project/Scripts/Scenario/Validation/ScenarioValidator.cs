public enum ScenarioValidationErrorType
{
    None,
    ScenarioInvalid,   // error in scenario data
    LevelInvalid       // the level does not match the scenario
}

public readonly struct ScenarioValidationResult
{
    public readonly bool IsValid;
    public readonly ScenarioValidationErrorType ErrorType;
    public readonly string Message;

    public ScenarioValidationResult(bool isValid, ScenarioValidationErrorType errorType, string message)
    {
        IsValid = isValid;
        ErrorType = errorType;
        Message = message;
    }

    public static ScenarioValidationResult Ok() => new(true, ScenarioValidationErrorType.None, null);
}

public static class ScenarioValidator
{
    public static ScenarioValidationResult Validate(ScenarioDefinition scenario, LevelObjectRegistry levelObjectRegistry)
    {
        if (scenario == null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.ScenarioInvalid, "Scenario is not selected");
        }

        if (levelObjectRegistry == null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, "LevelObjectRegistry is missing");
        }

        // Validating scenario
        string scenarioErrorMessage = ValidateScenario(scenario);
        if (scenarioErrorMessage != null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.ScenarioInvalid, scenarioErrorMessage);
        }

        // Validating level
        foreach (ScenarioObjectRule objectRule in scenario.objectRules)
        {
            int count = CountObjects(objectRule.placeableObject, levelObjectRegistry);

            if (count < objectRule.minCount)
            {
                return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, 
                    $"Scenario requires at least {objectRule.minCount} object(s) of type '{objectRule.placeableObject.name}'");
            }

            if (objectRule.maxCount >= 0 && count > objectRule.maxCount)
            {
                return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid,
                    $"Scenario allows at most {objectRule.maxCount} object(s) of type '{objectRule.placeableObject.name}'");
            }
        }

        // Validating scenario specific data
        if (scenario.specificValidator != null)
        {
            return scenario.specificValidator.Validate(levelObjectRegistry);
        }

        return ScenarioValidationResult.Ok();
    }

    private static string ValidateScenario(ScenarioDefinition scenario)
    {
        if (string.IsNullOrEmpty(scenario.scenarioId))
        {
            return "Scenario has empty scenarioId";
        }

        if (scenario.objectRules == null || scenario.objectRules.Count == 0)
        {
            return "Scenario objectRules list is null or empty";
        }

        foreach (ScenarioObjectRule objectRule in scenario.objectRules)
        {
            if (string.IsNullOrEmpty(objectRule.placeableObject.objectId))
            {
                return $"Scenario '{scenario.scenarioId}' contains rule with empty objectId";
            }

            if (objectRule.maxCount >= 0 && objectRule.maxCount < objectRule.minCount)
            {
                return $"Scenario '{scenario.scenarioId}' has invalid rule for '{objectRule.placeableObject.name}': maxCount < minCount";
            }
        }

        return null;
    }

    private static int CountObjects(PlaceableObjectDefinition placeableObject, LevelObjectRegistry levelObjectRegistry)
    {
        int count = 0;

        foreach (LevelObject levelObject in levelObjectRegistry.LevelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject.SourcePlaceableObject == placeableObject)
            {
                count++;
            }
        }

        return count;
    }
}

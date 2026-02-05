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
    public static ScenarioValidationResult Validate(ScenarioDefinition scenario, LevelRuntimeRegistry levelRuntimeRegistry)
    {
        if (scenario == null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.ScenarioInvalid, "Scenario is not selected");
        }

        if (levelRuntimeRegistry == null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, "LevelRuntimeRegistry is missing");
        }

        // Validating scenario data
        string scenarioError = ValidateScenarioDefinition(scenario);
        if (scenarioError != null)
        {
            return new ScenarioValidationResult(false, ScenarioValidationErrorType.ScenarioInvalid, scenarioError);
        }

        // Validating level
        foreach (ScenarioObjectRule objectRule in scenario.objectRules)
        {
            int count = CountObjects(objectRule.objectId, levelRuntimeRegistry);

            if (count < objectRule.minCount)
            {
                return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid, 
                    $"Scenario requires at least {objectRule.minCount} object(s) of type '{objectRule.objectId}'");
            }

            if (objectRule.maxCount >= 0 && count > objectRule.maxCount)
            {
                return new ScenarioValidationResult(false, ScenarioValidationErrorType.LevelInvalid,
                    $"Scenario allows at most {objectRule.maxCount} object(s) of type '{objectRule.objectId}'");
            }
        }

        return ScenarioValidationResult.Ok();
    }

    private static string ValidateScenarioDefinition(ScenarioDefinition scenario)
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
            if (string.IsNullOrEmpty(objectRule.objectId))
            {
                return $"Scenario '{scenario.scenarioId}' contains rule with empty objectId";
            }

            if (objectRule.maxCount >= 0 && objectRule.maxCount < objectRule.minCount)
            {
                return $"Scenario '{scenario.scenarioId}' has invalid rule for '{objectRule.objectId}': maxCount < minCount";
            }
        }

        return null;
    }

    private static int CountObjects(string objectId, LevelRuntimeRegistry levelRuntimeRegistry)
    {
        int count = 0;

        foreach (LevelObject levelObject in levelRuntimeRegistry.LevelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject.SourceData.objectId == objectId)
            {
                count++;
            }
        }

        return count;
    }
}

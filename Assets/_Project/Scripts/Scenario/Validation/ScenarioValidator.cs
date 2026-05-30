public enum ScenarioValidationErrorType
{
    None,
    ScenarioInvalid,   // error in scenario data
    LevelInvalid       // the level does not match the scenario
}

public static class ScenarioValidator
{
    public static ScenarioValidationResult Validate(ScenarioDefinition scenario, LevelObjectRegistry levelObjectRegistry)
    {
        ScenarioValidationResult result = new();

        if (scenario == null)
        {
            result.Add(
                new ValidationIssue(
                    ScenarioValidationErrorType.ScenarioInvalid,
                    "validation_scenario_not_selected"));

            return result;
        }

        if (levelObjectRegistry == null)
        {
            result.Add(
                new ValidationIssue(
                    ScenarioValidationErrorType.LevelInvalid,
                    "validation_registry_missing"));

            return result;
        }

        ValidateScenarioDefinition(scenario, result);

        ValidateLevelObjects(
            scenario,
            levelObjectRegistry,
            result);

        scenario.specificValidator?.Validate(
            levelObjectRegistry,
            result);

        return result;
    }

    private static void ValidateScenarioDefinition(ScenarioDefinition scenario, ScenarioValidationResult result)
    {
        if (string.IsNullOrEmpty(scenario.scenarioId))
        {
            result.Add(
                new ValidationIssue(
                    ScenarioValidationErrorType.ScenarioInvalid,
                    "validation_empty_scenario_id"));
        }

        if (scenario.objectRules == null || scenario.objectRules.Count == 0)
        {
            result.Add(
                new ValidationIssue(
                    ScenarioValidationErrorType.ScenarioInvalid,
                    "validation_empty_rules"));
        }

        if (scenario.objectRules == null)
        {
            return;
        }

        foreach (ScenarioObjectRule rule in scenario.objectRules)
        {
            if (rule.placeableObject == null)
            {
                result.Add(
                    new ValidationIssue(
                        ScenarioValidationErrorType.ScenarioInvalid,
                        "validation_null_placeable_object"));

                continue;
            }

            if (string.IsNullOrEmpty(rule.placeableObject.objectId))
            {
                result.Add(
                    new ValidationIssue(
                        ScenarioValidationErrorType.ScenarioInvalid,
                        "validation_empty_object_id",
                        rule.placeableObject.name));
            }

            if (rule.maxCount >= 0 && rule.maxCount < rule.minCount)
            {
                result.Add(
                    new ValidationIssue(
                        ScenarioValidationErrorType.ScenarioInvalid,
                        "validation_invalid_min_max",
                        rule.placeableObject.name));
            }
        }
    }

    private static void ValidateLevelObjects(ScenarioDefinition scenario, LevelObjectRegistry registry, ScenarioValidationResult result)
    {
        foreach (ScenarioObjectRule rule in scenario.objectRules)
        {
            int count =
                CountObjects(
                    rule.placeableObject,
                    registry);

            if (count < rule.minCount)
            {
                result.Add(
                    new ValidationIssue(
                        ScenarioValidationErrorType.LevelInvalid,
                        "validation_missing_objects",
                        rule.placeableObject.GetName(),
                        rule.minCount));
            }

            if (rule.maxCount >= 0 &&
                count > rule.maxCount)
            {
                result.Add(
                    new ValidationIssue(
                        ScenarioValidationErrorType.LevelInvalid,
                        "validation_too_many_objects",
                        rule.placeableObject.GetName(),
                        rule.maxCount));
            }
        }
    }

    private static int CountObjects(PlaceableObjectDefinition placeableObject, LevelObjectRegistry registry)
    {
        int count = 0;

        foreach (LevelObject levelObject in registry.LevelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject.SourcePlaceableObject ==
                placeableObject)
            {
                count++;
            }
        }

        return count;
    }
}

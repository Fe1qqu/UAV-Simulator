public static class ScenarioRuntimeFactory
{
    public static IScenarioRuntime Create(ScenarioDefinition scenarioDefinition)
    {
        if (scenarioDefinition == null)
        {
            return null;
        }

        return scenarioDefinition.scenarioId switch
        {
            "free_flight" => new FreeFlightScenarioRuntime(),
            "reach_target" => new ReachTargetScenarioRuntime(),
            _ => null
        };
    }
}

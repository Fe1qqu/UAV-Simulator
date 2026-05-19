using System;

public interface IScenarioRuntime
{
    event Action<IScenarioRuntime> ScenarioCompleted;
    event Action<IScenarioRuntime> ScenarioFailed;

    void Initialize(LevelObjectRegistry levelObjectRegistry, DroneControllerBase droneController);
    void StartScenario();
    void TickScenario();
    void FixedTickScenario();
    void ResetScenario();
    void DisposeScenario();
}

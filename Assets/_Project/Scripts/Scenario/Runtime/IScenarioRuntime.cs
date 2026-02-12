using System;

public interface IScenarioRuntime
{
    event Action<IScenarioRuntime> ScenarioCompleted;

    void Initialize(LevelRuntimeRegistry registry, DroneControllerBase droneController);
    void StartScenario();
    void TickScenario();
    void ResetScenario();
    void DisposeScenario();
}

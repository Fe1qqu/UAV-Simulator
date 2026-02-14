using System;

public interface IScenarioRuntime
{
    event Action<IScenarioRuntime> ScenarioCompleted;

    void Initialize(LevelObjectRegistry levelObjectRegistry, DroneControllerBase droneController);
    void StartScenario();
    void TickScenario();
    void ResetScenario();
    void DisposeScenario();
}

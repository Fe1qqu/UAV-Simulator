using System;

public interface IScenarioRuntime
{
    /// <summary>
    /// Fired when active gameplay is over and player input should be disabled.
    /// This may happen earlier than ScenarioCompleted/ScenarioFailed.
    /// </summary>
    event Action<IScenarioRuntime> GameplayConcluded;

    event Action<IScenarioRuntime> ScenarioCompleted;
    event Action<IScenarioRuntime> ScenarioFailed;

    bool IsGameplayConcluded { get; }

    void Initialize(LevelObjectRegistry levelObjectRegistry, UAVControllerBase uavController);
    void StartScenario();
    void TickScenario();
    void FixedTickScenario();
    void ResetScenario();
    void DisposeScenario();
}

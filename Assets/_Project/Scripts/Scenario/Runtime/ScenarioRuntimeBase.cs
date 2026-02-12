using System;

public abstract class ScenarioRuntimeBase : IScenarioRuntime
{
    protected LevelRuntimeRegistry registry;
    protected DroneControllerBase droneController;

    public event Action<IScenarioRuntime> ScenarioCompleted;

    private bool isCompleted;

    public virtual void Initialize(LevelRuntimeRegistry registry, DroneControllerBase droneController)
    {
        this.registry = registry;
        this.droneController = droneController;
        isCompleted = false;
    }

    public abstract void StartScenario();
    public abstract void TickScenario();
    public abstract void ResetScenario();
    public virtual void DisposeScenario() { }

    protected void CompleteScenario()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;

        ScenarioCompleted?.Invoke(this);
    }
}

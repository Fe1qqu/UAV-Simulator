using System;

public abstract class ScenarioRuntimeBase : IScenarioRuntime
{
    protected LevelObjectRegistry levelObjectRegistry;
    protected DroneControllerBase droneController;

    public event Action<IScenarioRuntime> ScenarioCompleted;

    private bool isCompleted;

    public virtual void Initialize(LevelObjectRegistry levelObjectRegistry, DroneControllerBase droneController)
    {
        this.levelObjectRegistry = levelObjectRegistry;
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

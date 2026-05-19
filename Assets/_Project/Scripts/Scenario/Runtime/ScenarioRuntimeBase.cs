using UnityEngine;
using System;

public abstract class ScenarioRuntimeBase : ScriptableObject, IScenarioRuntime
{
    protected LevelObjectRegistry levelObjectRegistry;
    protected DroneControllerBase droneController;

    public event Action<IScenarioRuntime> ScenarioCompleted;
    public event Action<IScenarioRuntime> ScenarioFailed;

    private bool isCompleted;
    private bool isFailed;

    public virtual void Initialize(LevelObjectRegistry levelObjectRegistry, DroneControllerBase droneController)
    {
        this.levelObjectRegistry = levelObjectRegistry;
        this.droneController = droneController;
        isCompleted = false;

        OnInitialize();
    }

    public void StartScenario()
    {
        isCompleted = false;
        StartScenarioInternal();
    }

    public void TickScenario()
    {
        if (isCompleted)
        {
            return;
        }

        TickScenarioInternal();
    }

    public void FixedTickScenario()
    {
        if (isCompleted)
        {
            return;
        }

        FixedTickScenarioInternal();
    }

    public void ResetScenario()
    {
        isCompleted = false;
        isFailed = false;
        ResetScenarioInternal();
    }

    public void DisposeScenario()
    {
        DisposeScenarioInternal();
        isCompleted = false;
    }

    protected void CompleteScenario()
    {
        if (isCompleted || isFailed)
        {
            return;
        }

        isCompleted = true;
        ScenarioCompleted?.Invoke(this);
    }

    protected void FailScenario()
    {
        if (isCompleted || isFailed)
        {
            return;
        }

        isFailed = true;
        ScenarioFailed?.Invoke(this);
    }

    protected virtual void OnInitialize() { }
    protected abstract void StartScenarioInternal();
    protected virtual void TickScenarioInternal() { }
    protected virtual void FixedTickScenarioInternal() { }
    protected abstract void ResetScenarioInternal();
    protected virtual void DisposeScenarioInternal() { }
}

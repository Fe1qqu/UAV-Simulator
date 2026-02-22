using UnityEngine;
using System;

public abstract class ScenarioRuntimeBase : ScriptableObject, IScenarioRuntime
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
        ResetScenarioInternal();
    }

    public void DisposeScenario()
    {
        DisposeScenarioInternal();
        isCompleted = false;
    }

    protected void CompleteScenario()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        ScenarioCompleted?.Invoke(this);
    }

    protected virtual void OnInitialize() { }
    protected abstract void StartScenarioInternal();
    protected virtual void TickScenarioInternal() { }
    protected virtual void FixedTickScenarioInternal() { }
    protected abstract void ResetScenarioInternal();
    protected virtual void DisposeScenarioInternal() { }
}

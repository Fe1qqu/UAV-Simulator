using UnityEngine;
using System;

public abstract class ScenarioRuntimeBase : ScriptableObject, IScenarioRuntime
{
    protected LevelObjectRegistry levelObjectRegistry;
    protected UAVControllerBase uavController;

    public event Action<IScenarioRuntime> GameplayConcluded;

    public event Action<IScenarioRuntime> ScenarioCompleted;
    public event Action<IScenarioRuntime> ScenarioFailed;

    public bool IsGameplayConcluded => isGameplayConcluded;

    private bool isCompleted;
    private bool isFailed;

    private bool isGameplayConcluded;

    public virtual void Initialize(LevelObjectRegistry levelObjectRegistry, UAVControllerBase uavController)
    {
        this.levelObjectRegistry = levelObjectRegistry;
        this.uavController = uavController;

        OnInitialize();
    }

    public void StartScenario()
    {
        isCompleted = false;
        isFailed = false;
        isGameplayConcluded = false;
        StartScenarioInternal();
    }

    public void TickScenario()
    {
        if (isCompleted || isFailed)
        {
            return;
        }

        TickScenarioInternal();
    }

    public void FixedTickScenario()
    {
        if (isCompleted || isFailed)
        {
            return;
        }

        FixedTickScenarioInternal();
    }

    public void ResetScenario()
    {
        isCompleted = false;
        isFailed = false;
        isGameplayConcluded = false;
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

    protected void ConcludeGameplay()
    {
        if (isGameplayConcluded)
        {
            return;
        }

        isGameplayConcluded = true;
        GameplayConcluded?.Invoke(this);
    }

    protected virtual void OnInitialize() { }
    protected abstract void StartScenarioInternal();
    protected virtual void TickScenarioInternal() { }
    protected virtual void FixedTickScenarioInternal() { }
    protected abstract void ResetScenarioInternal();
    protected virtual void DisposeScenarioInternal() { }
}

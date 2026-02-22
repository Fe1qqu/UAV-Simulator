using UnityEngine;

public abstract class MainMenuScreenBase : MonoBehaviour, IBackHandler
{
    protected MainMenuStateMachine stateMachine;

    public virtual void Initialize(MainMenuStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    // === PUBLIC API (NOT OVERRIDED) ===

    public void OnShow()
    {
        BackDispatcher.RegisterHandler(this);
        OnShowInternal();
    }

    public void OnShow(object context)
    {
        BackDispatcher.RegisterHandler(this);
        OnShowInternal(context);
    }

    public void OnHide()
    {
        OnHideInternal();
        BackDispatcher.UnregisterHandler(this);
    }

    // === EXTENSION POINTS ===
    protected virtual void OnShowInternal() { }
    protected virtual void OnShowInternal(object context) => OnShowInternal();
    protected virtual void OnHideInternal() { }

    // Each screen MUST define its own behavior Back
    public abstract bool OnBack();
}

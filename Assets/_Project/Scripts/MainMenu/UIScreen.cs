using UnityEngine;

public abstract class UIScreen : MonoBehaviour
{
    protected MainMenuStateMachine stateMachine;

    public virtual void Initialize(MainMenuStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void OnShow() { }
    public virtual void OnShow(object context) => OnShow();
    public virtual void OnHide() { }
}

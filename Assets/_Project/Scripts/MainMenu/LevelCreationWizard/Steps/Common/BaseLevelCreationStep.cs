using UnityEngine;

/// <summary>
/// Base class for a step in the level creation wizard.
/// </summary>
public abstract class BaseLevelCreationStep : MonoBehaviour
{
    protected MainMenuContext MainMenuContext { get; private set; }

    /// <summary>
    /// Called once by the wizard to inject dependencies.
    /// </summary>
    public void Initialize(MainMenuContext mainMenuContext)
    {
        MainMenuContext = mainMenuContext;
        OnInitialized();
    }

    protected virtual void OnInitialized() { }

    /// <summary>
    /// Called when this step is shown in the wizard.
    /// Can be overridden to initialize step UI.
    /// </summary>
    public virtual void OnStepShown() { }

    /// <summary>
    /// Validates the step before moving to the next one.
    /// Return true if validation passes, false to block navigation.
    /// </summary>
    /// <returns>True if step is valid, otherwise false.</returns>
    public virtual bool ValidateStep()
    {
        return true;
    }
}

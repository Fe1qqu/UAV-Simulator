using UnityEngine;

public enum PauseCloseMode
{
    ResumeGameplay,
    SceneExit
}

public abstract class PauseMenuBase : MonoBehaviour, IBackHandler
{
    [SerializeField] protected GameObject pauseMenuRoot;

    protected bool isOpen;
    public bool IsOpen => isOpen; 

    protected virtual void Awake()
    {
        if (pauseMenuRoot == null)
        {
            Debug.LogError($"[{GetType().Name}] PauseMenuRoot is not assigned.");
            return;
        }

        pauseMenuRoot.SetActive(false);
    }

    public virtual void Open()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;

        PauseManager.SetPaused(true);

        BackDispatcher.RegisterHandler(this);

        pauseMenuRoot.SetActive(true);

        OnOpened();
    }

    public virtual void Close(PauseCloseMode mode = PauseCloseMode.ResumeGameplay)
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        PauseManager.SetPaused(false);

        BackDispatcher.UnregisterHandler(this);

        if (mode == PauseCloseMode.ResumeGameplay)
        {
            pauseMenuRoot.SetActive(false);
        }

        OnClosed();
    }

    public bool OnBack()
    {
        Close();
        return true;
    }

    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}

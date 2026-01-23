using UnityEngine;

public abstract class BasePauseMenu : MonoBehaviour, IBackHandler
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
        pauseMenuRoot.SetActive(true);

        PauseManager.SetPaused(true);

        BackDispatcher.Instance.Register(this);

        OnOpened();
    }

    public virtual void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        pauseMenuRoot.SetActive(false);

        PauseManager.SetPaused(false);

        BackDispatcher.Instance.Unregister(this);

        OnClosed();
    }

    public bool OnBack()
    {
        if (!isOpen)
        {
            return false;
        }

        Close();
        return true;
    }

    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}

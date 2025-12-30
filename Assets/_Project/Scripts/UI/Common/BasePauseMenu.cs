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
        Time.timeScale = 0f;

        OnOpened();
        BackDispatcher.Instance.Register(this);
    }

    public virtual void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;

        OnClosed();
        BackDispatcher.Instance.Unregister(this);
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

using UnityEngine;
using System.Collections.Generic;

public class MainMenuStateMachine : MonoBehaviour
{
    [SerializeField] private List<UIScreen> screens;

    private UIScreen currentScreen;

    private void Awake()
    {
        foreach (var screen in screens)
        {
            screen.Initialize(this);
            screen.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Show<MainMenuScreen>();
    }

    public void Show<T>() where T : UIScreen
    {
        UIScreen target = screens.Find(screen => screen is T);

        if (target == null)
        {
            Debug.LogError($"Screen {typeof(T).Name} not found!");
            return;
        }

        if (currentScreen == target)
        {
            return;
        }

        if (currentScreen != null)
        {
            currentScreen.OnHide();
            currentScreen.gameObject.SetActive(false);
        }

        currentScreen = target;
        currentScreen.gameObject.SetActive(true);
        currentScreen.OnShow();
    }

    public void Show<T>(object context = null) where T : UIScreen
    {
        UIScreen target = screens.Find(screen => screen is T);

        if (target == null)
        {
            Debug.LogError($"Screen {typeof(T).Name} not found!");
            return;
        }

        if (currentScreen == target)
        {
            return;
        }

        if (currentScreen != null)
        {
            currentScreen.OnHide();
            currentScreen.gameObject.SetActive(false);
        }

        currentScreen = target;
        currentScreen.gameObject.SetActive(true);

        if (context != null)
        {
            currentScreen.OnShow(context);
        }
        else
        {
            currentScreen.OnShow();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

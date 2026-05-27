using UnityEngine;
using System.Collections.Generic;

public class MainMenuStateMachine : MonoBehaviour
{
    [SerializeField] private List<MainMenuScreenBase> screens;

    private MainMenuScreenBase currentScreen;

    private void Awake()
    {
        foreach (MainMenuScreenBase screen in screens)
        {
            screen.Initialize(this);
            screen.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Show<MainMenuScreen>();
    }

    public void Show<T>(object context = null) where T : MainMenuScreenBase
    {
        MainMenuScreenBase target = screens.Find(screen => screen is T);

        if (target == null)
        {
            Debug.LogError($"[MainMenuStateMachine] Screen {typeof(T).Name} not found.");
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

        UINavigatorContext.Instance.ResetSelection();

        currentScreen = target;
        currentScreen.gameObject.SetActive(true);

        currentScreen.OnShow(context);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

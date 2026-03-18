using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public PlaySession Play { get; private set; } = new();
    public LevelEditorSession LevelEditor { get; private set; } = new();

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[GameSession] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GameStateManager.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.StateChanged -= OnStateChanged;
    }

    #endregion

    #region State Handling

    private void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                ClearPlay();
                ClearLevelEditor();
                break;
        }
    }

    #endregion

    #region API

    public void ClearPlay()
    {
        Play.Clear();
    }

    public void ClearLevelEditor()
    {
        LevelEditor.Clear();
    }

    #endregion
}

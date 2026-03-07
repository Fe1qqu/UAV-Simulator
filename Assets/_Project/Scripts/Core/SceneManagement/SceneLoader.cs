using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static void LoadLevelEditor()
    {
        SceneManager.LoadScene("LevelEditor");
    }

    public static void LoadPlay()
    {
        SceneManager.LoadScene("Play");
    }
}

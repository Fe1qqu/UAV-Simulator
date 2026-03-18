using System.Collections.Generic;

public class SceneDatabase
{
    public class Scenes
    {
        public const string AppStart = "AppStart";
        public const string MainMenu = "MainMenu";
        public const string LevelEditor = "LevelEditor";
        public const string Play = "Play";
    }
}

public static class SceneRegistry
{
    public static readonly Dictionary<string, SceneDefinition> Scenes = new()
    {
        {
            SceneDatabase.Scenes.AppStart,
            new SceneDefinition
            {
                SceneName = SceneDatabase.Scenes.AppStart,
                InputMode = InputMode.None,
                UseOverlay = false
            }
        },
        {
            SceneDatabase.Scenes.MainMenu,
            new SceneDefinition
            {
                SceneName = SceneDatabase.Scenes.MainMenu,
                InputMode = InputMode.UI,
                UseOverlay = false
            }
        },
        {
            SceneDatabase.Scenes.Play,
            new SceneDefinition
            {
                SceneName = SceneDatabase.Scenes.Play,
                InputMode = InputMode.Play,
                UseOverlay = true
            }
        },
        {
            SceneDatabase.Scenes.LevelEditor,
            new SceneDefinition
            {
                SceneName = SceneDatabase.Scenes.LevelEditor,
                InputMode = InputMode.LevelEditor,
                UseOverlay = true
            }
        }
    };

    public static bool TryGet(string sceneName, out SceneDefinition sceneDefinition)
    {
        return Scenes.TryGetValue(sceneName, out sceneDefinition);
    }
}

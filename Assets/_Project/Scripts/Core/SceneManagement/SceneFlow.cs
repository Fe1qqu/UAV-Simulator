using UnityEngine;
using System.Threading.Tasks;

public static class SceneFlow
{
    public static Task ToScene(string sceneName)
    {
        if (!SceneRegistry.TryGet(sceneName, out var sceneDefinition))
        {
            Debug.LogError($"[SceneFlow] Scene '{sceneName}' not registered.");
            return ToMainMenu();
        }

        var transition = SceneController.Instance
            .NewTransition()
            .Load(SceneSlot.Main, sceneDefinition.SceneName, true)
            .WithInputMode(sceneDefinition.InputMode);

        if (sceneDefinition.UseOverlay)
        {
            transition.WithOverlay();
        }

        return transition.Perform();
    }

    public static Task ToAppStart() => ToScene(SceneDatabase.Scenes.AppStart);
    public static Task ToMainMenu() => ToScene(SceneDatabase.Scenes.MainMenu);
    public static Task ToPlay() => ToScene(SceneDatabase.Scenes.Play);
    public static Task ToLevelEditor() => ToScene(SceneDatabase.Scenes.LevelEditor);
}

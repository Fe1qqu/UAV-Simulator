using UnityEngine;
using System.Threading.Tasks;

public static class SceneFlow
{
    public static Task ToScene(string sceneName, bool? forceOverlay = null)
    {
        if (!SceneRegistry.TryGet(sceneName, out SceneDefinition sceneDefinition))
        {
            Debug.LogError($"[SceneFlow] Scene '{sceneName}' not registered.");
            return ToMainMenu();
        }

        var transition = SceneController.Instance
            .NewTransition()
            .Load(SceneSlot.Main, sceneDefinition.SceneName, true)
            .WithInputMode(sceneDefinition.InputMode);

        bool useOverlay = forceOverlay ?? sceneDefinition.UseOverlay;

        if (useOverlay)
        {
            transition.WithOverlay();
        }

        return transition.Perform();
    }

    public static Task ToAppStart() => ToScene(SceneDatabase.Scenes.AppStart);
    public static Task ToMainMenu(bool? overlay = null) => ToScene(SceneDatabase.Scenes.MainMenu, overlay);
    public static Task ToPlay(bool? overlay = null) => ToScene(SceneDatabase.Scenes.Play, overlay);
    public static Task ToLevelEditor(bool? overlay = null) => ToScene(SceneDatabase.Scenes.LevelEditor, overlay);
}

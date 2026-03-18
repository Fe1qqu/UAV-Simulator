using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class AppStartManager : MonoBehaviour
{
    const string SceneKey = "BOOTSTRAP_RETURN_SCENE";

    private void Start()
    {
        _ = Run();
    }

    private async Task Run()
    {
        await SceneController.Instance.WaitUntilFree();
        await LoadNextScene();
    }

    private async Task LoadNextScene()
    {
#if UNITY_EDITOR

        string scenePath = EditorPrefs.GetString(SceneKey, null);

        if (!string.IsNullOrEmpty(scenePath))
        {
            EditorPrefs.DeleteKey(SceneKey);

            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (SceneRegistry.Scenes.ContainsKey(sceneName))
            {
                await SceneFlow.ToScene(sceneName);
            }

            return;
        }

#endif
        
        await SceneFlow.ToMainMenu();
    }
}

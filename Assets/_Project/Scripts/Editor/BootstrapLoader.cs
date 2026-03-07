using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class BootstrapLoader
{
    const string SceneKey = "BOOTSTRAP_RETURN_SCENE";

    static BootstrapLoader()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var scene = EditorSceneManager.GetActiveScene();

            if (scene.buildIndex != 0)
            {
                EditorPrefs.SetInt(SceneKey, scene.buildIndex);

                EditorSceneManager.playModeStartScene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(
                        EditorBuildSettings.scenes[0].path
                    );
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}

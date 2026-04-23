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

            // If the scene is NOT in BuildSettings → DO NOT touch it at all
            if (scene.buildIndex == -1)
            {
                return;
            }

            // If this is NOT the first scene (Core)
            if (scene.buildIndex != 0)
            {
                EditorPrefs.SetString(SceneKey, scene.path);

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

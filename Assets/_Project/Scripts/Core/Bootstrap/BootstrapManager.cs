using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootstrapManager : MonoBehaviour
{
    const string SceneKey = "BOOTSTRAP_RETURN_SCENE";

    private void Start()
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
#if UNITY_EDITOR

        int sceneIndex = EditorPrefs.GetInt(SceneKey, -1);

        if (sceneIndex > 0)
        {
            EditorPrefs.DeleteKey(SceneKey);
            SceneManager.LoadScene(sceneIndex);
            return;
        }

#endif

        SceneManager.LoadScene(1);
    }
}

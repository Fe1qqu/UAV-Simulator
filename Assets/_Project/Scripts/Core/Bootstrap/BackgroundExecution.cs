using UnityEngine;

public static class BackgroundExecution
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnableRunInBackground()
    {
        Application.runInBackground = true;
        //Debug.Log("[BackgroundExecution] Application.runInBackground enabled");
    }
}

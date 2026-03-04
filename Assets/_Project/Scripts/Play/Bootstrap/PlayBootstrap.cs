using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.Play;
        GameSettings.Instance.EnterGameplay();
        //Debug.Log("[GameMode] Play.");
    }
}

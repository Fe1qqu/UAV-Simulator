using UnityEngine;

public class PlayBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameModeContext.Current = GameMode.Play;
        //Debug.Log("[GameMode] Play.");
    }
}

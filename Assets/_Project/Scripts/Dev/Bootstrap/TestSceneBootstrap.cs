using UnityEngine;

public class TestSceneBootstrap : MonoBehaviour
{
    [SerializeField] private InputMode mode = InputMode.Play;

    private void Start()
    {
        InputModeController.Instance.SetMode(mode);
    }
}

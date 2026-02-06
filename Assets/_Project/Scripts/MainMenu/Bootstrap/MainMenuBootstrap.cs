using UnityEngine;
using System.Threading.Tasks;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;

    private void Awake()
    {
        if (mainMenuRoot == null)
        {
            Debug.LogError("[MainMenuBootstrap] MainMenuRoot is not assigned.");
            return;
        }
    }

    private void Start()
    {
        _ = BootstrapAsync();
    }

    private async Task BootstrapAsync()
    {
        mainMenuRoot.SetActive(false);

        await GameSettings.Instance.LocalizationReadyTask;

        mainMenuRoot.SetActive(true);
    }
}

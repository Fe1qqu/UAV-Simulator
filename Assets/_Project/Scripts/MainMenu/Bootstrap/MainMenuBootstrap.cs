using UnityEngine;
using UnityEngine.Localization.Settings;
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

        GameSettings.Instance.EnterMainMenu();
    }

    private void Start()
    {
        _ = BootstrapAsync();
    }

    private async Task BootstrapAsync()
    {
        mainMenuRoot.SetActive(false);

        await LocalizationSettings.InitializationOperation.Task;

        mainMenuRoot.SetActive(true);
    }
}

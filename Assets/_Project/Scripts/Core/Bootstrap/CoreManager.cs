using UnityEngine;
using System.Threading.Tasks;

public class CoreManager : MonoBehaviour
{
    private void Start()
    {
        _ = Run();
    }

    private async Task Run()
    {
        // Core setup

        GameSettings.Instance.ApplyAuto(SettingAutoApply.OnAppBoot);

        await SceneFlow.ToAppStart();
    }
}

using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

[RequireComponent(typeof(FadeManager))]
public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private FadeManager fadeManager;

    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        fadeManager = GetComponent<FadeManager>();
    }

    public async Task FadeInAsync()
    {
        Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        await fadeManager.FadeInAsync(fadeInDuration, cancellationTokenSource.Token);
    }

    public async Task FadeOutAsync()
    {
        Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        await fadeManager.FadeOutAsync(fadeOutDuration, cancellationTokenSource.Token);
    }

    private void Cancel()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}

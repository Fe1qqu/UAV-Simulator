using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class FadeManager : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private CancellationTokenSource fadeCancellationTokenSource;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        CancelFade();
    }

    /// <summary>
    /// Instantly sets alpha to given value.
    /// </summary>
    public void SetAlpha(float value)
    {
        CancelFade();
        canvasGroup.alpha = value;
    }

    /// <summary>
    /// Smooth fade in to alpha 1.
    /// </summary>
    public Task FadeInAsync(float duration, CancellationToken token)
    {
        return FadeToAsync(1f, duration, token);
    }

    /// <summary>
    /// Smooth fade out to alpha 0.
    /// </summary>
    public Task FadeOutAsync(float duration, CancellationToken token)
    {
        return FadeToAsync(0f, duration, token);
    }

    private async Task FadeToAsync(float target, float duration, CancellationToken externalToken)
    {
        CancelFade();
        fadeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        CancellationToken token = fadeCancellationTokenSource.Token;

        float start = canvasGroup.alpha;
        float time = 0f;

        try
        {
            while (time < duration)
            {
                token.ThrowIfCancellationRequested();

                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                canvasGroup.alpha = Mathf.Lerp(start, target, t);

                await Task.Yield();
            }

            canvasGroup.alpha = target;
        }
        catch (OperationCanceledException)
        {
            // expected
        }
    }

    private void CancelFade()
    {
        if (fadeCancellationTokenSource != null)
        {
            fadeCancellationTokenSource.Cancel();
            fadeCancellationTokenSource.Dispose();
            fadeCancellationTokenSource = null;
        }
    }
}

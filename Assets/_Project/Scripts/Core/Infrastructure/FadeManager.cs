using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class FadeManager : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Instantly sets alpha to given value.
    /// </summary>
    public void SetAlpha(float value)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        canvasGroup.alpha = value;
    }

    /// <summary>
    /// Smooth fade in to alpha 1.
    /// </summary>
    public void FadeIn(float duration)
    {
        StartFade(1f, duration);
    }

    /// <summary>
    /// Smooth fade out to alpha 0.
    /// </summary>
    public void FadeOut(float duration)
    {
        StartFade(0f, duration);
    }

    private void StartFade(float target, float duration)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(FadeCoroutine(target, duration));
    }

    private IEnumerator FadeCoroutine(float target, float duration)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        canvasGroup.alpha = target;
        currentCoroutine = null;
    }

    // ================== Async versions ==================

    /// <summary>
    /// Smooth fade in asynchronously.
    /// </summary>
    public Task FadeInAsync(float duration)
    {
        return FadeToAsync(1f, duration);
    }

    /// <summary>
    /// Smooth fade out asynchronously.
    /// </summary>
    public Task FadeOutAsync(float duration)
    {
        return FadeToAsync(0f, duration);
    }

    /// <summary>
    /// Core async fade logic.
    /// </summary>
    private async Task FadeToAsync(float target, float duration)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / duration);
            await Task.Yield();
        }

        canvasGroup.alpha = target;
    }
}

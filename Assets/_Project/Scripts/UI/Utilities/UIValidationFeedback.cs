using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIValidationFeedback : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Graphic targetGraphic;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;

    [SerializeField] private Color errorColor = new(1f, 0f, 0f, 0.6f);

    [SerializeField] private Color successColor = new(0f, 1f, 0f, 0.6f);

    [Header("Animation")]
    [SerializeField] private int blinkCount = 3;

    [SerializeField] private float blinkInterval = 0.12f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetGraphic == null)
        {
            Debug.LogError("[UIValidationFeedback] TargetGraphic is not assigned.");
        }
    }

    public void SetNormal()
    {
        StopActiveAnimation();

        if (targetGraphic != null)
        {
            targetGraphic.color = normalColor;
        }
    }

    public void PlayErrorFlash()
    {
        PlayFlash(errorColor);
    }

    public void PlaySuccessFlash()
    {
        PlayFlash(successColor);
    }

    private void PlayFlash(Color flashColor)
    {
        if (targetGraphic == null)
        {
            return;
        }

        StopActiveAnimation();
        activeCoroutine = StartCoroutine(FlashRoutine(flashColor));
    }

    private IEnumerator FlashRoutine(Color flashColor)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            targetGraphic.color = flashColor;
            yield return new WaitForSeconds(blinkInterval);

            targetGraphic.color = normalColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        targetGraphic.color = normalColor;
        activeCoroutine = null;
    }

    private void StopActiveAnimation()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }
}

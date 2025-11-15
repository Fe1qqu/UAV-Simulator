using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

/// <summary>
/// Manages the display of tooltips in the UI.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Canvas containing the tooltip UI.")]
    public Canvas tooltipCanvas;

    [Tooltip("CanvasGroup controlling tooltip visibility and fade.")]
    public CanvasGroup tooltipCanvasGroup;

    [Tooltip("RectTransform of the tooltip panel.")]
    public RectTransform tooltipRect;

    [Tooltip("Text component displaying the tooltip message.")]
    public TextMeshProUGUI tooltipText;

    [Header("Settings")]
    [Tooltip("Offset from mouse position for tooltip.")]
    public Vector2 tooltipOffset = new Vector2(12f, -8f);

    [Tooltip("Speed of tooltip fade-in.")]
    public float tooltipFadeSpeed = 3.0f;

    [Tooltip("Delay before showing the tooltip.")]
    public float tooltipDelay = 0.1f;

    private bool isVisible;
    private bool dragMode = false;
    private Coroutine showCoroutine;

    /// <summary>
    /// Returns true if the tooltip system is in drag mode.
    /// </summary>
    public bool IsInDragMode => dragMode;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[TooltipManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tooltipCanvas == null)
        {
            Debug.LogError("[TooltipManager] Missing reference to tooltipCanvas.");
        }

        if (tooltipCanvasGroup == null)
        {
            Debug.LogError("[TooltipManager] Missing reference to tooltipCanvasGroup.");
        }

        if (tooltipRect == null)
        {
            Debug.LogError("[TooltipManager] Missing references to tooltipRect.");
        }

        if (tooltipText == null)
        {
            Debug.LogError("[TooltipManager] Missing references to tooltipText.");
        }

        Hide();
    }

    private void Update()
    {
        if (!isVisible)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        // Fade in the tooltip smoothly
        if (tooltipCanvasGroup.alpha < 1)
        {
            tooltipCanvasGroup.alpha += Time.deltaTime * tooltipFadeSpeed;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenPosition = mousePosition + tooltipOffset;

        Vector2 tooltipSize = tooltipRect.sizeDelta * tooltipCanvas.scaleFactor;

        float maxX = Screen.width - tooltipSize.x;
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, tooltipSize.y, Screen.height);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
             tooltipCanvas.transform as RectTransform,
             screenPosition,
             tooltipCanvas.worldCamera,
             out var localPoint);

        tooltipRect.localPosition = localPoint;
    }

    /// <summary>
    /// Shows the tooltip with the given message.
    /// <param name="message">Text to display in the tooltip.</param>
    /// <param name="force">If true, ignores dragMode suppression (used by UIDragContext).</param>
    /// </summary>
    public void Show(string message, bool force = false)
    {
        if (dragMode && !force)
        {
            return;
        }    

        //Debug.Log($"tooltipShow message: {message}");

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        isVisible = true;
        showCoroutine = StartCoroutine(ShowWithDelay(message));
    }

    private IEnumerator ShowWithDelay(string message)
    {
        yield return new WaitForSeconds(tooltipDelay);

        // If Hide was called during the waiting period, we do not show
        if (!isVisible)
        {
            yield break;
        }

        if (tooltipCanvasGroup != null)
        {
            tooltipCanvasGroup.alpha = 0;
        }

        tooltipText.text = message;
        tooltipRect.gameObject.SetActive(true);
        showCoroutine = null;
    }

    /// <summary>
    /// Hides the currently displayed tooltip.
    /// </summary>
    public void Hide()
    {
        //Debug.Log("tooltipHide");

        tooltipRect.gameObject.SetActive(false);
        isVisible = false;
    }

    /// <summary>
    /// Enters drag mode, suppressing normal tooltips.
    /// </summary>
    public void EnterDragMode()
    {
        dragMode = true;
    }

    /// <summary>
    /// Exits drag mode, allowing normal tooltips to appear again.
    /// </summary>
    public void ExitDragMode()
    {
        dragMode = false;
    }
}

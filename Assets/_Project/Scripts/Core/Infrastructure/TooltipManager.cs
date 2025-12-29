using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Manages the display of tooltips in the UI.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Canvas containing the tooltip UI.")]
    [SerializeField] private Canvas tooltipCanvas;

    [Tooltip("RectTransform of the tooltip panel.")]
    [SerializeField] private RectTransform tooltipRect;

    [Tooltip("Localize event that updates tooltip text.")]
    [SerializeField] private LocalizeStringEvent tooltipLocalizeEvent;

    [Header("Settings")]
    [Tooltip("Offset from mouse position for tooltip.")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(12f, -8f);

    [Tooltip("Speed of tooltip fade-in.")]
    [SerializeField] private float tooltipFadeSpeed = 0.1f;

    [Tooltip("Delay before showing the tooltip.")]
    [SerializeField] private float tooltipDelay = 0.1f;

    private FadeManager fadeManager;

    private bool isVisible;
    private bool dragMode = false;

    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Returns true if the tooltip system is in drag mode.
    /// </summary>
    public bool IsInDragMode => dragMode;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[TooltipManager] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tooltipCanvas == null)
        {
            Debug.LogError("[TooltipManager] TooltipCanvas is not assigned.");
        }

        if (tooltipCanvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogError("[TooltipManager] WorldSpace canvas is not supported.");
        }

        if (tooltipRect == null)
        {
            Debug.LogError("[TooltipManager] TooltipRect is not assigned.");
        }

        if (tooltipLocalizeEvent == null)
        {
            Debug.LogError("[TooltipManager] TooltipLocalizeEvent is not assigned.");
        }

        fadeManager = tooltipRect.GetComponent<FadeManager>();
        if (fadeManager == null)
        {
            Debug.LogWarning("[TooltipManager] FadeManager not found on tooltipRect.");
        }

        HideImmediate();
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

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenPosition = mousePosition + tooltipOffset;

        Vector2 tooltipSize = tooltipRect.sizeDelta;

        // Clamp inside screen bounds
        screenPosition.x = Mathf.Clamp(
            screenPosition.x,
            0,
            Screen.width - tooltipSize.x
        );

        screenPosition.y = Mathf.Clamp(
            screenPosition.y,
            tooltipSize.y,
            Screen.height
        );

        Camera cam = null;
        if (tooltipCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = tooltipCanvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipCanvas.transform as RectTransform,
            screenPosition,
            cam,
            out Vector2 localPoint
        );

        tooltipRect.localPosition = localPoint;
    }

    /// <summary>
    /// Shows the tooltip with the message.
    /// <param name="localizedKey">Localized key used to display text in the tooltip.</param>
    /// <param name="force">If true, ignores dragMode suppression (used by UIDragContext).</param>
    /// </summary>
    public void Show(LocalizedString localizedKey, bool force = false)
    {
        if (dragMode && !force)
        {
            return;
        }

        //Debug.Log($"tooltipShow message: {localizedKey}");

        CancelPendingShow();
        cancellationTokenSource = new CancellationTokenSource();

        _ = ShowAsync(localizedKey, cancellationTokenSource.Token);
    }

    private async Task ShowAsync(LocalizedString localizedKey, CancellationToken token)
    {
        isVisible = true;

        try
        {
            int delayMs = Mathf.RoundToInt(tooltipDelay * 1000f);
            await Task.Delay(delayMs, token);

            if (token.IsCancellationRequested || !isVisible)
            {
                return;
            }

            tooltipLocalizeEvent.StringReference = localizedKey;
            tooltipLocalizeEvent.RefreshString();

            tooltipRect.gameObject.SetActive(true);
            fadeManager.SetAlpha(0f);
            fadeManager.FadeIn(tooltipFadeSpeed);
        }
        catch (TaskCanceledException)
        {
            // expected
        }
    }

    /// <summary>
    /// Hides the currently displayed tooltip.
    /// </summary>
    public void Hide()
    {
        //Debug.Log("tooltipHide");

        CancelPendingShow();
        HideImmediate();
    }

    private void HideImmediate()
    {
        isVisible = false;
        tooltipRect.gameObject.SetActive(false);
    }

    private void CancelPendingShow()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
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

    private void OnDestroy()
    {
        CancelPendingShow();
    }
}

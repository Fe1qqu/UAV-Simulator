using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using System.Linq;
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
    [SerializeField] private Canvas canvas;

    [Tooltip("RectTransform of the tooltip panel.")]
    [SerializeField] private RectTransform rectTransform;

    [Tooltip("Localize event that updates tooltip text.")]
    [SerializeField] private LocalizeStringEvent localizeStringEvent;

    [SerializeField] private FadeManager fadeManager;

    [SerializeField] private ScriptableObject[] resolvers;

    [Header("Settings Providers")]
    private TooltipSettingsPipeline pipeline;

    private bool isVisible;
    private bool dragMode;
    private TooltipRequest currentRequest;
    private TooltipSettings currentTooltipSettings;
    private CancellationTokenSource showCancellationTokenSource;

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

        if (canvas == null)
        {
            Debug.LogError("[TooltipManager] Canvas is not assigned.");
        }

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogError("[TooltipManager] WorldSpace canvas is not supported.");
        }

        if (rectTransform == null)
        {
            Debug.LogError("[TooltipManager] RectTransform is not assigned.");
        }

        if (localizeStringEvent == null)
        {
            Debug.LogError("[TooltipManager] LocalizeStringEvent is not assigned.");
        }

        if (fadeManager == null)
        {
            Debug.LogWarning("[TooltipManager] FadeManager is not assigned.");
        }

        pipeline = new TooltipSettingsPipeline(resolvers.OfType<ITooltipSettingsResolver>());

        HideImmediate();
    }

    private void OnDestroy()
    {
        CancelPendingShow();
    }

    private void Update()
    {
        if (!isVisible || currentTooltipSettings == null || currentTooltipSettings.displayMode != TooltipDisplayMode.FollowPointer)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenPosition = mousePosition + currentTooltipSettings.tooltipOffset;

        Vector2 tooltipSize = rectTransform.sizeDelta * canvas.scaleFactor;

        // Clamp inside screen bounds
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width - tooltipSize.x);
        screenPosition.y = Mathf.Clamp(screenPosition.y, tooltipSize.y, Screen.height);

        Camera camera = canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            camera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;
    }

    public void Show(ITooltipSource source)
    {
        if (source == null)
        {
            Debug.LogWarning("[TooltipManager] Show called with null ITooltipSource.");
            return;
        }

        TooltipRequest request = source.CreateTooltipRequest();

        if (!request.isValid)
        {
            Debug.LogWarning($"[TooltipManager] TooltipRequest is invalid. Source: {source.GetType().Name}");
            return;
        }

        if (dragMode && !request.force)
        {
            //Debug.Log($"[TooltipManager] Tooltip suppressed due to drag mode. Context: {(request.context != null ? request.context.name : "null")}");
            return;
        }

        currentRequest = request;
        currentTooltipSettings = pipeline.Resolve(request);

        if (currentTooltipSettings == null)
        {
            Debug.LogError($"[TooltipManager] TooltipSettingsPipeline returned null settings. Context: {(request.context != null ? request.context.name : "null")}");
            return;
        }

        CancelPendingShow();
        showCancellationTokenSource = new CancellationTokenSource();

        _ = ShowAsync(request.text, showCancellationTokenSource.Token);
    }

    public void Hide()
    {
        _ = HideAsync();
    }

    private async Task ShowAsync(LocalizedString localizedKey, CancellationToken token)
    {
        try
        {
            int delayMs = Mathf.RoundToInt(currentTooltipSettings.delay * 1000f);
            await Task.Delay(delayMs, token);

            token.ThrowIfCancellationRequested();

            localizeStringEvent.StringReference = localizedKey;
            localizeStringEvent.RefreshString();

            isVisible = true;
            rectTransform.gameObject.SetActive(true);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            ApplyFixedAnchorIfNeeded();

            fadeManager.SetAlpha(0f);
            await fadeManager.FadeInAsync(currentTooltipSettings.fadeInDuration, token);
        }
        catch (TaskCanceledException)
        {
            // expected
        }
    }

    private void ApplyFixedAnchorIfNeeded()
    {
        if (currentTooltipSettings.displayMode != TooltipDisplayMode.FixedAnchor)
        {
            return;
        }

        if (currentRequest.fixedAnchor == null)
        {
            Debug.LogError($"[TooltipManager] FixedAnchor is null for '{currentRequest.context.name}'.");
            return;
        }

        Vector3 basePosition = currentRequest.fixedAnchor.position;
        Vector2 tooltipSize = rectTransform.sizeDelta * canvas.scaleFactor;
        Vector2 offset = currentTooltipSettings.fixedOffset;

        switch (currentTooltipSettings.anchorSide)
        {
            case TooltipAnchorSide.Top:
                offset += new Vector2(-tooltipSize.x * 0.5f, tooltipSize.y);
                break;
            case TooltipAnchorSide.Bottom:
                offset += new Vector2(-tooltipSize.x * 0.5f, 0);
                break;
            case TooltipAnchorSide.Left:
                offset += new Vector2(-tooltipSize.x, tooltipSize.y * 0.5f);
                break;
            case TooltipAnchorSide.Right:
                offset += new Vector2(0f, tooltipSize.y * 0.5f);
                break;
        }

        rectTransform.position = basePosition + (Vector3)offset;
    }

    private async Task HideAsync()
    {
        CancelPendingShow();

        if (!isVisible)
        {
            return;
        }

        isVisible = false;

        try
        {
            if (fadeManager != null && currentTooltipSettings != null)
            {
                await fadeManager.FadeOutAsync(currentTooltipSettings.fadeOutDuration, CancellationToken.None);
            }
        }
        finally
        {
            HideImmediate();
        }
    }

    private void HideImmediate()
    {
        //isVisible = false;
        rectTransform.gameObject.SetActive(false);
        currentTooltipSettings = null;
        currentRequest = default;
    }

    private void CancelPendingShow()
    {
        if (showCancellationTokenSource != null)
        {
            showCancellationTokenSource.Cancel();
            showCancellationTokenSource.Dispose();
            showCancellationTokenSource = null;
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
}

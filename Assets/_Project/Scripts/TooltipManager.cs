using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System.Collections;

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
            Debug.LogError("[TooltipManager] TooltipCanvas is not assigned.");
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

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        isVisible = true;
        showCoroutine = StartCoroutine(ShowWithDelay(localizedKey));
    }

    private IEnumerator ShowWithDelay(LocalizedString localizedKey)
    {
        yield return new WaitForSeconds(tooltipDelay);

        // If Hide was called during the waiting period, we do not show
        if (!isVisible)
        {
            yield break;
        }

        tooltipLocalizeEvent.StringReference = localizedKey;
        tooltipLocalizeEvent.RefreshString();

        fadeManager.SetAlpha(0);
        tooltipRect.gameObject.SetActive(true);
        fadeManager.FadeIn(tooltipFadeSpeed);

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

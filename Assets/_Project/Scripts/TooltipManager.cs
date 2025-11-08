using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 offset = new Vector2(12f, -8f);

    private Canvas canvas;
    private bool isVisible;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[TooltipManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        canvas = GetComponentInParent<Canvas>();

        if (tooltipRect == null || tooltipText == null)
        {
            Debug.LogError("[TooltipManager] Missing references to tooltipRect or tooltipText.");
        }

        Hide();
    }

    private void Update()
    {
        if (!isVisible || canvas == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenPosition = mousePosition + offset;

        Vector2 tooltipSize = tooltipRect.sizeDelta * canvas.scaleFactor;

        // Корректируем позицию, если тултип выходит за экран
        float maxX = Screen.width - tooltipSize.x;
        float maxY = Screen.height - tooltipSize.y;
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, tooltipSize.y, Screen.height);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out var localPoint);

        tooltipRect.localPosition = localPoint;
    }

    public void Show(string message)
    {
        if (tooltipRect == null || tooltipText == null)
        {
            return;
        }

        tooltipText.text = message;
        tooltipRect.gameObject.SetActive(true);
        isVisible = true;
    }

    public void Hide()
    {
        if (tooltipRect != null)
        {
            tooltipRect.gameObject.SetActive(false);
        }

        isVisible = false;
    }
}

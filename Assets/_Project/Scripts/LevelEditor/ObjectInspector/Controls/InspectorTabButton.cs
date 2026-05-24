using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class InspectorTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private UISelectionButtonVisual visual;

    [Header("Tab")]
    [SerializeField] private ObjectInspectorTab tab;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltipAnchor;
    [SerializeField] private LocalizedString tooltipLocalizedString;

    private ObjectInspectorController objectInspectorController;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[InspectorTabButton] Button is not assigned.");
        }

        if (visual == null)
        {
            Debug.LogError("[InspectorTabButton] Visual is not assigned.");
        }

        if (tooltipAnchor == null)
        {
            Debug.LogError("[InspectorTabButton] TooltipAnchor is not assigned.");
        }

        if (tooltipLocalizedString == null)
        {
            Debug.LogError("[InspectorTabButton] TooltipLocalizedString is not assigned.");
        }
    }

    public void Initialize(ObjectInspectorController objectInspectorController)
    {
        this.objectInspectorController = objectInspectorController;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        objectInspectorController.SelectTab(tab);
    }

    public void SetSelected(bool selected)
    {
        visual.SetSelected(selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.Hide();
        }
    }

    public TooltipRequest CreateTooltipRequest()
    {
        return new TooltipRequest
        {
            isValid = true,
            text = tooltipLocalizedString,
            context = gameObject,
            fixedAnchor = tooltipAnchor
        };
    }
}

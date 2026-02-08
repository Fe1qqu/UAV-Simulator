using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class InspectorTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource
{
    [SerializeField] private ObjectInspectorTab tab;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private Button button;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltipAnchor;
    [SerializeField] private LocalizedString tooltipLocalizationString;

    private ObjectInspector objectInspector;

    private void Awake()
    {
        if (selectionHighlight == null)
        {
            Debug.LogError("[InspectorTabButton] SelectionHighlight is not assigned.");
        }

        if (button == null)
        {
            Debug.LogError("[InspectorTabButton] Button is not assigned.");
        }

        if (tooltipAnchor == null)
        {
            Debug.LogError("[InspectorTabButton] TooltipAnchor is not assigned.");
        }

        if (tooltipLocalizationString == null)
        {
            Debug.LogError("[InspectorTabButton] TooltipLocalizationString is not assigned.");
        }
    }

    public void Initialize(ObjectInspector objectInspector)
    {
        this.objectInspector = objectInspector;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        objectInspector.SelectTab(tab);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
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
            text = tooltipLocalizationString,
            context = gameObject,
            fixedAnchor = tooltipAnchor
        };
    }
}

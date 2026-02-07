using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InspectorTabButton : MonoBehaviour
{
    [SerializeField] private ObjectInspectorTab tab;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private Button button;

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
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIObjectHierarchyItem : MonoBehaviour
{
    public event System.Action<LevelObject> OnDeleteRequested;
    public event System.Action<LevelObject> OnSelectRequested;

    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text nameText;

    [Header("Selection")]
    [SerializeField] private GameObject selectionHighlight;

    private LevelObject boundLevelObject;

    private void Awake()
    {
        if (selectButton == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] SelectButton is not assigned.");
        }

        if (deleteButton == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] DeleteButton is not assigned.");
        }

        if (nameText == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] NameText is not assigned.");
        }

        if (selectionHighlight == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] SelectionHighlight is not assigned.");
        }
    }

    private void OnEnable()
    {
        selectButton.onClick.AddListener(OnSelectClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveListener(OnSelectClicked);
        deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }

    public void Bind(LevelObject levelObject)
    {
        if (levelObject == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] Bind called with null LevelObject.");
            return;
        }

        boundLevelObject = levelObject;
    }

    private void OnSelectClicked()
    {
        if (boundLevelObject == null)
        {
            Debug.LogWarning("[UIObjectHierarchyItem] Select clicked but LevelObject is null.");
            return;
        }

        OnSelectRequested?.Invoke(boundLevelObject);
    }

    private void OnDeleteClicked()
    {
        if (boundLevelObject == null)
        {
            Debug.LogWarning("[UIObjectHierarchyItem] Delete clicked but LevelObject is null.");
            return;
        }

        OnDeleteRequested?.Invoke(boundLevelObject);
    }

    public void SetDisplayName(string displayName)
    {
        nameText.text = displayName;
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

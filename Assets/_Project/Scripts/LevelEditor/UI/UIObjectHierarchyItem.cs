using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class UIObjectHierarchyItem : MonoBehaviour
{
    public event System.Action<LevelObject> OnDeleteRequested;
    public event System.Action<LevelObject> OnSelectRequested;

    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private LocalizeStringEvent nameLocalizeStringEvent;

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

        if (nameLocalizeStringEvent == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] NameLocalizeStringEvent is not assigned.");
        }
    }

    public void Bind(LevelObject levelObject)
    {
        if (levelObject == null)
        {
            Debug.LogError("[UIObjectHierarchyItem] Bind called with null LevelObject.");
            return;
        }

        boundLevelObject = levelObject;

        if (levelObject.SourceData != null)
        {
            nameLocalizeStringEvent.StringReference = levelObject.SourceData.localizationKey;
            nameLocalizeStringEvent.RefreshString();
        }
        else
        {
            Debug.LogWarning($"[UIObjectHierarchyItem] LevelObject '{levelObject.name}' has no SourceData.");
        }

        selectButton.onClick.AddListener(OnSelectClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    public void Unbind()
    {
        selectButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        boundLevelObject = null;
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

    public void SetSelected(bool selected)
    {
        //nameLabel.color = selected ? Color.yellow : Color.white;
    }
}

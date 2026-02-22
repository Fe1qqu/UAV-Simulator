using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LevelSelectItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text scenarioNameText;
    [SerializeField] private GameObject selectionHighlight;

    private LevelCatalogEntry levelCatalogEntry;
    public LevelCatalogEntry Entry => levelCatalogEntry;

    private Action<LevelSelectItem> onSelected;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[LevelSelectItem] Button is not assigned.");
        }

        if (levelNameText == null)
        {
            Debug.LogError("[LevelSelectItem] LevelNameText is not assigned.");
        }

        if (scenarioNameText == null)
        {
            Debug.LogError("[LevelSelectItem] ScenarioNameText is not assigned.");
        }

        if (selectionHighlight == null)
        {
            Debug.LogError("[LevelSelectItem] SelectionHighlight is not assigned.");
        }
    }

    public void Setup(LevelCatalogEntry levelCatalogEntry, Action<LevelSelectItem> onSelected)
    {
        this.levelCatalogEntry = levelCatalogEntry;
        this.onSelected = onSelected;

        levelNameText.text = levelCatalogEntry.LevelData.levelName;
        scenarioNameText.text = levelCatalogEntry.ScenarioDisplayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected(this));
    }

    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
    }
}

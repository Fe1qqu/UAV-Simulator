using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LevelSelectItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject selectionHighlight;

    private LevelCatalogEntry levelCatalogEntry;
    public LevelCatalogEntry Entry => levelCatalogEntry;

    //private Action<LevelSelectItem> onSelected;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[LevelSelectItem] Button is not assigned.");
        }

        if (missionText == null)
        {
            Debug.LogError("[LevelSelectItem] MissionText is not assigned.");
        }

        if (scenarioText == null)
        {
            Debug.LogError("[LevelSelectItem] ScenarioText is not assigned.");
        }

        if (locationText == null)
        {
            Debug.LogError("[LevelSelectItem] LocationText is not assigned.");
        }

        if (deleteButton == null)
        {
            Debug.LogError("[LevelSelectItem] DeleteButton is not assigned.");
        }

        if (selectionHighlight == null)
        {
            Debug.LogError("[LevelSelectItem] SelectionHighlight is not assigned.");
        }
    }

    public void Setup(LevelCatalogEntry levelCatalogEntry, Action<LevelSelectItem> onSelected, Action<LevelSelectItem> onDelete)
    {
        this.levelCatalogEntry = levelCatalogEntry;
        //this.onSelected = onSelected;

        missionText.text = levelCatalogEntry.LevelData.levelName;
        scenarioText.text = levelCatalogEntry.ScenarioDisplayName;
        locationText.text = levelCatalogEntry.LocationDisplayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected(this));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this));
    }

    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}

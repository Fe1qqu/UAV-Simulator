using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class LevelSelectItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private GameObject selectionHighlight;

    private string filePath;
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

        if (selectionHighlight == null)
        {
            Debug.LogError("[LevelSelectItem] SelectionHighlight is not assigned.");
        }
    }

    public void Setup(string filePath, Action<LevelSelectItem> onSelected)
    {
        this.filePath = filePath;
        this.onSelected = onSelected;

        levelNameText.text = Path.GetFileNameWithoutExtension(filePath);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onSelected?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
    }

    public string GetFilePath()
    {
        return filePath;
    }
}

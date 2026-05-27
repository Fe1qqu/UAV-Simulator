using UnityEngine;

public class UISelectionButtonVisual : UIVisualBase
{
    [Header("References")]
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject outlineObject;

    [Header("Normal")]
    [SerializeField] private Color normalBackground = Color.white;
    [SerializeField] private Color normalOutline = Color.white;
    [SerializeField] private Color normalText = Color.white;
    [SerializeField] private Color normalIcon = Color.white;

    [Header("Selected")]
    [SerializeField] private Color selectedBackground = Color.white;
    [SerializeField] private Color selectedOutline = Color.white;
    [SerializeField] private Color selectedText = Color.white;
    [SerializeField] private Color selectedIcon = Color.white;

    public void SetSelected(bool selected)
    {
        ApplyColors(
            selected ? selectedText : normalText,
            selected ? selectedBackground : normalBackground,
            selected ? selectedOutline : normalOutline,
            selected ? selectedIcon : normalIcon
        );

        if (checkmark != null)
        {
            checkmark.SetActive(selected);
        }

        if (outlineObject != null)
        {
            outlineObject.SetActive(selected);
        }
    }
}

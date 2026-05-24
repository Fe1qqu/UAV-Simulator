using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonVisual : UIVisualBase
{
    [Header("Normal")]
    [SerializeField] private Color normalBackground = Color.white;
    [SerializeField] private Color normalOutline = Color.white;
    [SerializeField] private Color normalText = Color.white;
    [SerializeField] private Color normalIcon = Color.white;

    [Header("Disabled")]
    [SerializeField] private Color disabledBackground = Color.gray;
    [SerializeField] private Color disabledOutline = Color.gray;
    [SerializeField] private Color disabledText = Color.gray;
    [SerializeField] private Color disabledIcon = Color.gray;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
        Refresh();
    }

    public void Refresh()
    {
        bool interactable = button != null && button.interactable;

        ApplyColors(
            interactable ? normalText : disabledText,
            interactable ? normalBackground : disabledBackground,
            interactable ? normalOutline : disabledOutline,
            interactable ? normalIcon : disabledIcon
        );
    }
}

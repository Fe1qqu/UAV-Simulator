using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UIVisualBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected TMP_Text text;
    [SerializeField] protected Image background;
    [SerializeField] protected Image outline;
    [SerializeField] protected Graphic icon;

    protected void ApplyColors(Color? textColor = null, Color ? backgroundColor = null, Color? outlineColor = null, Color? iconColor = null)
    {
        if (text != null && textColor.HasValue)
        {
            text.color = textColor.Value;
        }

        if (background != null && backgroundColor.HasValue)
        {
            background.color = backgroundColor.Value;
        }

        if (outline != null && outlineColor.HasValue)
        {
            outline.color = outlineColor.Value;
        }

        if (icon != null && iconColor.HasValue)
        {
            icon.color = iconColor.Value;
        }
    }
}

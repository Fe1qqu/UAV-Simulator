using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private SelectableHighlight[] highlightParts;

    private void Awake()
    {
        highlightParts = GetComponentsInChildren<SelectableHighlight>();
        if (highlightParts == null || highlightParts.Length == 0)
        {
            Debug.LogWarning($"[SelectableObject] No SelectableHighlight components found in '{gameObject.name}' or its children.");
        }
    }

    public void Select()
    {
        foreach (SelectableHighlight highlight in highlightParts)
        {
            highlight.Show();
        }
    }

    public void Deselect()
    {
        foreach (SelectableHighlight highlight in highlightParts)
        {
            highlight.Hide();
        }
    }
}

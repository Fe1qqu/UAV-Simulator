using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private SelectableHighlight[] highlightParts;

    private void Awake()
    {
        highlightParts = GetComponentsInChildren<SelectableHighlight>();
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

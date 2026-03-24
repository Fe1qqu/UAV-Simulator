using UnityEngine;

public class SelectableObject : MonoBehaviour, ISelectable
{
    private ISelectionVisual[] visuals;

    private void Awake()
    {
        visuals = GetComponentsInChildren<ISelectionVisual>();
    }

    public void OnSelected()
    {
        foreach (ISelectionVisual visual in visuals)
        {
            visual.Show();
        }
    }

    public void OnDeselected()
    {
        foreach (ISelectionVisual visual in visuals)
        {
            visual.Hide();
        }
    }
}

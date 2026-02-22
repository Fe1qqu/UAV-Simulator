using UnityEngine;

public class LayerOutlineVisual : MonoBehaviour, ISelectionVisual
{
    [Tooltip("Layer applied when this mesh is highlighted.")]
    [SerializeField] private int highlightLayer = 6;

    // Original layer is stored at runtime
    private int originalLayer;

    private void Awake()
    {
        originalLayer = gameObject.layer;
    }

    public void Show()
    {
        gameObject.layer = highlightLayer;
    }

    public void Hide()
    {
        gameObject.layer = originalLayer;
    }
}

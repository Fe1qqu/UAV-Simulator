using UnityEngine;

/// <summary>
/// Defines a UI zone that affects drag behaviour:
/// - Shows tooltip when pointer is over it
/// - Can optionally cancel drag when released
/// Used by UIDragContext to give context-aware drag rules.
/// </summary>
[System.Serializable]
public class DragContextZone
{
    [Tooltip("Tag of the UI element or panel that acts as a drag context zone.")]
    public string tag;

    [Tooltip("Tooltip text displayed while the pointer hovers over this zone during a drag.")]
    public string tooltipText;

    [Tooltip("If enabled, releasing drag over this zone cancels the drag instead of placing.")]
    public bool cancelsDrag = true;
}

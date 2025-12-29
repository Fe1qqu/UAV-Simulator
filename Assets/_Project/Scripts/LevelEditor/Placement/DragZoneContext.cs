using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// Defines a UI zone that affects drag behaviour:
/// - Shows tooltip when pointer is over it
/// - Can optionally cancel drag when released
/// Used by UIDragContext to give context-aware drag rules.
/// </summary>
[System.Serializable]
public class DragZoneContext
{
    [Tooltip("Tag of the UI element or panel that acts as a drag context zone.")]
    public string tag;

    [Tooltip("Localized string shown as a tooltip when the pointer hovers over this zone during dragging.")]
    public LocalizedString tooltipLocalizedKey;

    [Tooltip("If enabled, releasing drag over this zone cancels the drag instead of placing.")]
    public bool cancelsDrag = true;
}

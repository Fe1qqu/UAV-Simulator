using UnityEngine;

[CreateAssetMenu(menuName = "UI/Tooltip/Group Resolver")]
public class GroupTooltipSettingsResolver : ScriptableObject, ITooltipSettingsResolver
{
    [System.Serializable]
    public struct Entry
    {
        public string tag;
        public TooltipSettings settings;
    }

    public Entry[] entries;
    public int Priority => 500;

    public TooltipSettings Resolve(TooltipRequest request)
    {
        if (request.context == null)
        {
            return null;
        }

        foreach (Entry entry in entries)
        {
            if (request.context.CompareTag(entry.tag))
            {
                return entry.settings;
            }
        }

        return null;
    }
}

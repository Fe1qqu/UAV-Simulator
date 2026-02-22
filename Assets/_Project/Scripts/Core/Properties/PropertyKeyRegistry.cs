using System.Collections.Generic;

public static class PropertyKeyRegistry
{
    private static readonly Dictionary<string, PropertyKey> _keys = new();

    public static void Register(PropertyKey key)
    {
        _keys[key.Name] = key;
    }

    public static bool TryGet(string name, out PropertyKey key)
    {
        return _keys.TryGetValue(name, out key);
    }

    public static PropertyKey Get(string name)
    {
        return _keys.TryGetValue(name, out var key) ? key : null;
    }
}

public abstract class PropertyKey
{
    public string Name { get; }

    protected PropertyKey(string name)
    {
        Name = name;
        PropertyKeyRegistry.Register(this);
    }
}

public sealed class PropertyKey<T> : PropertyKey
{
    public PropertyKey(string name) : base(name) { }
}

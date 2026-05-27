public abstract class PropertyKey
{
    public string Name { get; }

    protected PropertyKey(string name)
    {
        Name = name;
        PropertyKeyRegistry.Register(this);
    }

    public override bool Equals(object obj)
    {
        return obj is PropertyKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(PropertyKey a, PropertyKey b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Name == b.Name;
    }

    public static bool operator !=(PropertyKey a, PropertyKey b)
    {
        return !(a == b);
    }
}

public sealed class PropertyKey<T> : PropertyKey
{
    public PropertyKey(string name) : base(name) { }
}

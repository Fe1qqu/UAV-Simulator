using UnityEngine;

public static class LevelPropertyKeys
{
    public static readonly PropertyKey<int> Index = new("index");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() { }
}

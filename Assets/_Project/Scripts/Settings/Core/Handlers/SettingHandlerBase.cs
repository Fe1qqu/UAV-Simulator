using UnityEngine;

public abstract class SettingHandlerBase : ScriptableObject, ISettingHandler
{
    public abstract void Apply(SettingInstance setting);
}

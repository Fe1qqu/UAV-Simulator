using System;

public interface IGameSetting
{
    string Id { get; }

    event Action<object> OnRuntimeValueChanged;

    object GetRuntimeValue();
    void SetRuntimeValue(object value);

    void Apply();
    void Save();
    void Load();
}

using System;

public interface IGameSetting
{
    string Id { get; }

    object GetValue();
    void SetValue(object value);

    void Apply();
    void Save();
    void Load();

    event Action<object> OnValueChanged;
}
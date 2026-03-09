using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Settings/Settings Database")]
public class SettingsDatabase : ScriptableObject
{
    [SerializeField] private List<SettingDefinition> settings = new();

    public IReadOnlyList<SettingDefinition> Settings => settings;

    public SettingDefinition GetById(string id)
    {
        return settings.Find(setting => setting.Id == id);
    }
}

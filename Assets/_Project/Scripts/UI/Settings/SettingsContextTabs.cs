using System.Collections.Generic;

public enum SettingsContext
{
    MainMenu,
    PauseMenu,
    Editor
}

[System.Serializable]
public class SettingsContextTabs
{
    public SettingsContext context;
    public List<string> tabIds = new List<string>();
}

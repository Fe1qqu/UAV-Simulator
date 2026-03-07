using System;
using System.Collections.Generic;

public enum SettingsContext
{
    MainMenu,
    PauseMenu,
    LevelEditor
}

[Serializable]
public class SettingsContextTabs
{
    public SettingsContext context;
    public List<string> tabIds = new();
}

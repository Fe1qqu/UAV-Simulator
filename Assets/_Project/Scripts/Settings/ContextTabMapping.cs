using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сопоставляет список pageId, которые будут видны в указанном контексте.
/// pageId должны совпадать с SettingsPage.PageId на страницах в PagesContainer.
/// </summary>
[System.Serializable]
public class ContextTabMapping
{
    public SettingsContext context;
    [Tooltip("Список PageId (строк) в порядке отображения табов.")]
    public List<string> pageIds = new List<string>();
}

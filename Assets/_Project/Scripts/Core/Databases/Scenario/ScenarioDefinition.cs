using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Scenario Definition")]
public class ScenarioDefinition : ScriptableObject
{
    [Header("Identity")]
    public string scenarioId;
    public LocalizedString localizedString;

    [Header("Level Editor Availability")]
    public List<ScenarioCategoryRule> availableCategories = new();

    [Header("Level Editor Validation")]
    public List<ScenarioObjectRule> objectRules = new();
    public ScenarioSpecificValidator specificValidator;

    [Header("Runtime")]
    public ScenarioRuntimeBase runtime;
}

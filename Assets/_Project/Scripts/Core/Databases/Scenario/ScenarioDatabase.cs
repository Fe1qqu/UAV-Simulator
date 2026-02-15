using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Scenario Database")]
public class ScenarioDatabase : ScriptableObject
{
    public List<ScenarioDefinition> scenarios = new();

    public ScenarioDefinition GetById(string id)
    {
        return scenarios.Find(scenario => scenario.scenarioId == id);
    }
}

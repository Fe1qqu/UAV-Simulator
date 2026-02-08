using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScenarioDatabase", menuName = "Game Data/Scenario Database")]
public class ScenarioDatabase : ScriptableObject
{
    public List<ScenarioDefinition> scenarios = new();

    public ScenarioDefinition GetById(string id)
    {
        return scenarios.Find(scenario => scenario.scenarioId == id);
    }
}

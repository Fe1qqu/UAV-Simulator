using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Data/Scenarios Database")]
public class ScenariosDatabase : ScriptableObject
{
    public List<ScenarioDefinition> scenarios = new();

    public ScenarioDefinition GetById(string id)
    {
        return scenarios.Find(scenario => scenario.scenarioId == id);
    }
}

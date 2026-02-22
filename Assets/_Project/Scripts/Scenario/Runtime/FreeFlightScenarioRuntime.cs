using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Free Flight")]
public class FreeFlightScenarioRuntime : ScenarioRuntimeBase
{
    protected override void StartScenarioInternal()
    {
        // Intentionally empty
        // Free flight has no objectives
    }

    protected override void ResetScenarioInternal() { }
}

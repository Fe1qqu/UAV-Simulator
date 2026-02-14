using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Free Flight")]
public class FreeFlightScenarioRuntime : ScenarioRuntimeBase
{
    public override void StartScenario()
    {
        // Intentionally empty
        // Free flight has no objectives
    }

    public override void TickScenario() { }
    public override void ResetScenario() { }
}

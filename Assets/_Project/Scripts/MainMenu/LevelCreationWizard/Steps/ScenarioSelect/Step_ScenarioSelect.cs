using UnityEngine;

[RequireComponent(typeof(ScenarioSelection))]
public class Step_ScenarioSelect : BaseLevelCreationStep
{
    private ScenarioSelection scenarioSelection;

    private void Awake()
    {
        scenarioSelection = GetComponent<ScenarioSelection>();
    }

    public override bool ValidateStep()
    {
        ScenarioDefinition scenarioDefinition = scenarioSelection.GetSelectedScenario();
        if (scenarioDefinition == null)
        {
            Debug.LogWarning("[Step_ScenarioSelect] No scenario selected.");
            return false;
        }

        GameSettings.Instance.CurrentEditorSession.SelectedScenarioId = scenarioDefinition.scenarioId;
        return true;
    }
}

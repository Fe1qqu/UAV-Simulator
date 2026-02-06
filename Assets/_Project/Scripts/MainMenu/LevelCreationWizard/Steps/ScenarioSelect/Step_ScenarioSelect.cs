using UnityEngine;

[RequireComponent(typeof(ScenarioSelection))]
public class Step_ScenarioSelect : BaseLevelCreationStep
{
    private ScenarioSelection scenarioSelection;

    protected override void OnInitialized()
    {
        if (scenarioSelection == null)
        {
            scenarioSelection = GetComponent<ScenarioSelection>();
        }

        scenarioSelection.SetDatabase(MainMenuContext.ScenarioDatabase);
    }

    public override void OnStepShown()
    {
        scenarioSelection.BuildIfNeeded();
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

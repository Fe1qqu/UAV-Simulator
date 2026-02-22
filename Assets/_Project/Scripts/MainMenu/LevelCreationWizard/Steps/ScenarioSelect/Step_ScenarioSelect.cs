using UnityEngine;

[RequireComponent(typeof(ScenarioSelection))]
public class Step_ScenarioSelect : LevelCreationWizardStepBase
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
        ScenarioDefinition scenario = scenarioSelection.GetSelectedScenario();
        if (scenario == null)
        {
            Debug.LogWarning("[Step_ScenarioSelect] No scenario selected.");
            return false;
        }

        GameSettings.Instance.CurrentEditorSession.SelectedScenarioId = scenario.scenarioId;
        return true;
    }
}

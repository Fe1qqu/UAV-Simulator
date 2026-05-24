using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using System.Collections.Generic;

/// <summary>
/// Handles displaying a list of scenarios and selecting one from the UI.
/// Generates buttons dynamically from a <see cref="ScenarioDatabase"/>.
/// </summary>
public class ScenarioSelection : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform of the ScrollView content where scenario buttons will be instantiated.")]
    [SerializeField] private Transform contentParent;

    [Tooltip("Prefab used for each scenario button.")]
    [SerializeField] private GameObject scenarioButtonPrefab;

    // Currently selected scenario data
    private ScenarioDefinition selectedScenario;

    private readonly List<(Button button, UISelectionButtonVisual visual)> createdButtons = new();

    private ScenariosDatabase scenariosDatabase;
    private bool isBuilt;

    private void Awake()
    {
        if (contentParent == null)
        {
            Debug.LogError("[ScenarioSelection] ContentParent is not assigned.");
        }

        if (scenarioButtonPrefab == null)
        {
            Debug.LogError("[ScenarioSelection] ScenarioButtonPrefab is not assigned.");
        }
    }

    public void SetDatabase(ScenariosDatabase scenariosDatabase)
    {
        this.scenariosDatabase = scenariosDatabase;
    }

    public void BuildIfNeeded()
    {
        if (isBuilt)
        {
            return;
        }

        PopulateList();
        SelectDefault();
        isBuilt = true;
    }

    private void SelectDefault()
    {
        OnScenarioSelected(scenariosDatabase.scenarios[0], createdButtons[0].button);
    }

    private void PopulateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        createdButtons.Clear();

        foreach (ScenarioDefinition scenario in scenariosDatabase.scenarios)
        {
            GameObject scenarioButtonInstance = Instantiate(scenarioButtonPrefab, contentParent);
            if (scenarioButtonInstance == null)
            {
                Debug.LogError("[ScenarioSelection] Failed to instantiate scenario button prefab.");
                continue;
            }

            if (!scenarioButtonInstance.TryGetComponent(out Button button))
            {
                Debug.LogError("[ScenarioSelection] Button component not found on prefab.");
                continue;
            }

            scenarioButtonInstance.SetActive(true);

            LocalizeStringEvent localizeStringEvent = scenarioButtonInstance.GetComponentInChildren<LocalizeStringEvent>();
            if (localizeStringEvent != null)
            {
                localizeStringEvent.StringReference = scenario.localizedString;
                //localizeStringEvent.RefreshString();
            }
            else
            {
                Debug.LogWarning($"[ScenarioSelection] LocalizeStringEvent not found for scenario button.");
            }

            if (!scenarioButtonInstance.TryGetComponent(out UISelectionButtonVisual visual))
            {
                Debug.LogError("[ScenarioSelection] UISelectionButtonVisual not found.");
                continue;
            }

            visual.SetSelected(false);

            button.onClick.AddListener(() => OnScenarioSelected(scenario, button));
            createdButtons.Add((button, visual));
        }
    }

    private void OnScenarioSelected(ScenarioDefinition scenario, Button clickedButton)
    {
        selectedScenario = scenario;

        foreach (var (button, visual) in createdButtons)
        {
            visual.SetSelected(button == clickedButton);
        }

        // Debug.Log($"[ScenarioSelection] Selected: {scenario.scenarioId}");
    }

    public ScenarioDefinition GetSelectedScenario()
    {
        return selectedScenario;
    }
}

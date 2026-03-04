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

    // List of created buttons and their checkmarks
    private readonly List<(Button button, GameObject checkmark)> createdButtons = new();
    
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

            Transform iconTransform = scenarioButtonInstance.transform.Find("Icon");
            if (iconTransform == null)
            {
                Debug.LogWarning($"[ScenarioSelection] Icon transform not found for scenario '{scenario.scenarioId}'.");
            }
            else
            {
                if (!iconTransform.TryGetComponent<Image>(out var icon))
                {
                    Debug.LogWarning($"[ScenarioSelection] Image component missing on Icon for scenario '{scenario.scenarioId}'.");
                }
                else if (scenario.icon == null)
                {
                    Debug.LogWarning($"[ScenarioSelection] Scenario '{scenario.scenarioId}' has no icon assigned.");
                }
                else
                {
                    icon.sprite = scenario.icon;
                }
            }

            GameObject checkmarkGameObject = null;
            Transform checkmarkTransform = scenarioButtonInstance.transform.Find("Checkmark");
            if (checkmarkTransform == null)
            {
                Debug.LogWarning($"[ScenarioSelection] Checkmark object not found for scenario '{scenario.scenarioId}'.");
            }
            else
            {
                checkmarkGameObject = checkmarkTransform.gameObject;
                checkmarkGameObject.SetActive(false);
            }

            button.onClick.AddListener(() => OnScenarioSelected(scenario, button));
            createdButtons.Add((button, checkmarkGameObject));
        }
    }

    private void OnScenarioSelected(ScenarioDefinition scenario, Button clickedButton)
    {
        selectedScenario = scenario;

        foreach (var (button, checkmark) in createdButtons)
        {
            if (checkmark != null)
            {
                checkmark.SetActive(button == clickedButton);
            }
        }

        // Debug.Log($"[ScenarioSelection] Selected: {scenario.scenarioId}");
    }

    public ScenarioDefinition GetSelectedScenario()
    {
        return selectedScenario;
    }
}

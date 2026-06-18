using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using System.Collections.Generic;

/// <summary>
/// Handles displaying a list of locations and selecting one from the UI.
/// Generates buttons dynamically from a <see cref="LocationDatabase"/>.
/// </summary>
public class LocationSelection : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform of the ScrollView content where location buttons will be instantiated.")]
    [SerializeField] private Transform contentParent;

    [Tooltip("Prefab used for each location button.")]
    [SerializeField] private GameObject locationButtonPrefab;

    // Currently selected location data
    private LocationDefinition selectedLocation;

    private readonly List<(Button button, UISelectionButtonVisual visual)> createdButtons = new();

    private bool isBuilt;

    private void Awake()
    {
        if (contentParent == null)
        {
            Debug.LogError("[LocationSelection] ContentParent is not assigned.");
        }

        if (locationButtonPrefab == null)
        {
            Debug.LogError("[LocationSelection] LocationButtonPrefab is not assigned.");
        }
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
        OnLocationSelected(GameDataManager.Instance.Locations.locations[0], createdButtons[0].button);
    }

    private void PopulateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        createdButtons.Clear();

        foreach (LocationDefinition location in GameDataManager.Instance.Locations.locations)
        {
            GameObject locationButtonInstance = Instantiate(locationButtonPrefab, contentParent);
            if (locationButtonInstance == null)
            {
                Debug.LogError("[LocationSelection] Failed to instantiate location button prefab.");
                continue;
            }

            if (!locationButtonInstance.TryGetComponent<Button>(out var button))
            {
                Debug.LogError("[LocationSelection] Button component not found on prefab.");
                continue;
            }

            locationButtonInstance.SetActive(true);

            LocalizeStringEvent localizeStringEvent = locationButtonInstance.GetComponentInChildren<LocalizeStringEvent>();
            if (localizeStringEvent != null)
            {
                localizeStringEvent.StringReference = location.localizedString;
                //localizeStringEvent.RefreshString();
            }
            else
            {
                Debug.LogWarning($"[LocationSelection] LocalizeStringEvent not found for location button.");
            }

            if (!locationButtonInstance.TryGetComponent(out UISelectionButtonVisual visual))
            {
                Debug.LogError("[LocationSelection] UISelectionButtonVisual not found.");
                continue;
            }

            visual.SetSelected(false);

            button.onClick.AddListener(() => OnLocationSelected(location, button));
            createdButtons.Add((button, visual));
        }
    }

    private void OnLocationSelected(LocationDefinition location, Button clickedButton)
    {
        selectedLocation = location;

        foreach (var (button, visual) in createdButtons)
        {
            visual.SetSelected(button == clickedButton);
        }

        //Debug.Log($"[LocationSelection] Selected: {location.locationId}.");
    }

    public LocationDefinition GetSelectedLocation()
    {
        return selectedLocation;
    }
}

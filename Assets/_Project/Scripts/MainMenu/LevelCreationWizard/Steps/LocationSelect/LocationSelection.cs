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
    private LocationData selectedLocation;

    // List of created buttons and their checkmarks
    private readonly List<(Button button, GameObject checkmark)> createdButtons = new();

    private LocationDatabase locationDatabase;
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

    public void SetDatabase(LocationDatabase locationDatabase)
    {
        this.locationDatabase = locationDatabase;
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
        OnLocationSelected(locationDatabase.locations[0], createdButtons[0].button);
    }

    private void PopulateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        createdButtons.Clear();

        foreach (LocationData location in locationDatabase.locations)
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
                localizeStringEvent.StringReference = location.localizationKey;
                //localizeStringEvent.RefreshString();
            }
            else
            {
                Debug.LogWarning($"[LocationSelection] LocalizeStringEvent not found for location button.");
            }

            Transform iconTransform = locationButtonInstance.transform.Find("Icon");
            if (iconTransform == null)
            {
                Debug.LogWarning($"[LocationSelection] Icon transform not found for location '{location.locationId}'.");
            }
            else
            {
                if (!iconTransform.TryGetComponent<Image>(out var icon))
                {
                    Debug.LogWarning($"[LocationSelection] Image component missing on Icon for location '{location.locationId}'.");
                }
                else if (location.icon == null)
                {
                    Debug.LogWarning($"[LocationSelection] Location '{location.locationId}' has no icon assigned.");
                }
                else
                {
                    icon.sprite = location.icon;
                }
            }

            GameObject checkmarkGameObject = null;
            Transform checkmarkTransform = locationButtonInstance.transform.Find("Checkmark");
            if (checkmarkTransform == null)
            {
                Debug.LogWarning($"[LocationSelection] Checkmark object not found for location '{location.locationId}'.");
            }
            else
            {
                checkmarkGameObject = checkmarkTransform.gameObject;
                checkmarkGameObject.SetActive(false);
            }

            button.onClick.AddListener(() => OnLocationSelected(location, button));
            createdButtons.Add((button, checkmarkGameObject));
        }
    }

    private void OnLocationSelected(LocationData location, Button clickedButton)
    {
        selectedLocation = location;

        foreach (var (button, checkmark) in createdButtons)
        {
            if (checkmark != null)
            {
                checkmark.SetActive(button == clickedButton);
            }
        }

        //Debug.Log($"[LocationSelection] Selected: {location.locationId}.");
    }

    public LocationData GetSelectedLocation()
    {
        return selectedLocation;
    }
}

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
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

    [Header("Data")]
    [Tooltip("Database containing all available locations.")]
    [SerializeField] private LocationDatabase locationDatabase;

    // Currently selected location data
    private LocationData selectedLocation;

    // List of created buttons and their checkmarks
    private List<(Button button, GameObject checkmark)> createdButtons = new List<(Button, GameObject)>();

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

        if (locationDatabase == null || locationDatabase.locations.Count == 0)
        {
            Debug.LogError("[LocationSelection] LocationDatabase is missing or empty.");
        }
    }

    private void Start()
    {
        PopulateList();
        OnLocationSelected(locationDatabase.locations[0], createdButtons[0].button);
    }

    /// <summary>
    /// Populates the UI list with buttons for each location in the database.
    /// </summary>
    private void PopulateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        createdButtons.Clear();

        foreach (LocationData location in locationDatabase.locations)
        {
            GameObject btnObj = Instantiate(locationButtonPrefab, contentParent);
            if (btnObj == null)
            {
                Debug.LogError("[LocationSelection] Failed to instantiate location button prefab.");
                continue;
            }

            // Button
            if (!btnObj.TryGetComponent<Button>(out var btn))
            {
                Debug.LogError($"[LocationSelection] Button component not found on prefab for location '{location.localizationKey}'.");
                continue;
            }

            LocalizeStringEvent localizeEvent = btnObj.GetComponentInChildren<LocalizeStringEvent>();
            if (localizeEvent != null)
            {
                localizeEvent.StringReference = location.localizationKey;
                localizeEvent.RefreshString();
            }
            else
            {
                Debug.LogWarning($"[LocationSelection] LocalizeStringEvent not found for location button '{location.localizationKey.TableReference}:{location.localizationKey.TableEntryReference}'.");
            }

            // Icon
            Transform iconTransform = btnObj.transform.Find("Icon");
            if (iconTransform == null)
            {
                Debug.LogWarning($"[LocationSelection] Icon transform not found for location '{location.localizationKey}'.");
            }
            else
            {
                if (!iconTransform.TryGetComponent<Image>(out var icon))
                {
                    Debug.LogWarning($"[LocationSelection] Image component missing on Icon for location '{location.localizationKey}'.");
                }
                else if (location.preview == null)
                {
                    Debug.LogWarning($"[LocationSelection] Location '{location.localizationKey}' has no preview sprite assigned.");
                }
                else
                {
                    icon.sprite = location.preview;
                }
            }

            Transform checkmarkTransform = btnObj.transform.Find("Checkmark");
            GameObject checkmarkObj = null;
            if (checkmarkTransform == null)
            {
                Debug.LogWarning($"[LocationSelection] Checkmark object not found for location '{location.localizationKey}'.");
            }
            else
            {
                checkmarkObj = checkmarkTransform.gameObject;
                checkmarkObj.SetActive(false);
            }

            btn.onClick.AddListener(() => OnLocationSelected(location, btn));
            createdButtons.Add((btn, checkmarkObj));
        }
    }

    private void OnLocationSelected(LocationData location, Button clickedButton)
    {
        selectedLocation = location;

        foreach (var (button, checkmark) in createdButtons)
        {
            bool isSelected = button == clickedButton;

            if (checkmark != null)
            {
                checkmark.SetActive(isSelected);
            }
        }

        //Debug.Log($"[LocationSelection] Selected: {location.locationId}");
    }

    public LocationData GetSelectedLocation()
    {
        return selectedLocation;
    }
}

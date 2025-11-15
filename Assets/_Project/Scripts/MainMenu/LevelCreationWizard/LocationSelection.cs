using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles displaying a list of locations and selecting one from the UI.
/// Generates buttons dynamically from a <see cref="LocationDatabase"/>.
/// </summary>
public class LocationSelection : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform of the ScrollView content where location buttons will be instantiated.")]
    public Transform contentParent;

    [Tooltip("Prefab used for each location button.")]
    public GameObject locationButtonPrefab;

    [Header("Data")]
    [Tooltip("Database containing all available locations.")]
    public LocationDatabase locationDatabase;

    // Currently selected location name
    private string selectedLocation = null;

    // List of created buttons and their checkmarks
    private List<(Button button, GameObject checkmark)> createdButtons = new List<(Button, GameObject)>();

    private void Start()
    {
        PopulateList();

        if (locationDatabase.locations.Count > 0)
        {
            OnLocationSelected(locationDatabase.locations[0].name, createdButtons[0].button);
        }
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

        foreach (var location in locationDatabase.locations)
        {
            GameObject btnObj = Instantiate(locationButtonPrefab, contentParent);
            if (btnObj == null)
            {
                Debug.LogError("[LocationSelection] Failed to instantiate location button prefab.");
                continue;
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError($"[LocationSelection] Button component not found on prefab for location '{location.name}'.");
                continue;
            }

            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label == null)
            {
                Debug.LogWarning($"[LocationSelection] TMP_Text not found for location '{location.name}'.");
            }
            else
            {
                label.text = location.name;
            }

            Transform iconTransform = btnObj.transform.Find("Icon");
            if (iconTransform == null)
            {
                Debug.LogWarning($"[LocationSelection] Icon transform not found for location '{location.name}'.");
            }
            else
            {
                if (!iconTransform.TryGetComponent<Image>(out var icon))
                {
                    Debug.LogWarning($"[LocationSelection] Image component missing on Icon for location '{location.name}'.");
                }
                else if (location.preview == null)
                {
                    Debug.LogWarning($"[LocationSelection] Location '{location.name}' has no preview sprite assigned.");
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
                Debug.LogWarning($"[LocationSelection] Checkmark object not found for location '{location.name}'.");
            }
            else
            {
                checkmarkObj = checkmarkTransform.gameObject;
                checkmarkObj.SetActive(false);
            }

            btn.onClick.AddListener(() => OnLocationSelected(location.name, btn));
            createdButtons.Add((btn, checkmarkObj));
        }
    }

    /// <summary>
    /// Called when a location button is clicked. Updates the selected location and visual checkmarks.
    /// </summary>
    /// <param name="locationName">Name of the selected location.</param>
    /// <param name="clickedButton">Button that was clicked.</param>
    private void OnLocationSelected(string locationName, Button clickedButton)
    {
        selectedLocation = locationName;

        foreach (var (button, checkmark) in createdButtons)
        {
            bool isSelected = button == clickedButton;

            if (checkmark != null)
            {
                checkmark.SetActive(isSelected);
            }
        }

        //Debug.Log($"[LocationSelection] Selected: {selectedLocation}");
    }

    /// <summary>
    /// Returns the currently selected location name.
    /// </summary>
    /// <returns>Selected location name.</returns>
    public string GetSelectedLocation()
    {
        return selectedLocation;
    }
}

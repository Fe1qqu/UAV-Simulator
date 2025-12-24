using UnityEngine;
using UnityEngine.Localization;
using static UnityEditor.FilePathAttribute;

/// <summary>
/// Step 2 of the level creation wizard. Handles selecting a location for the level.
/// Requires a <see cref="LocationSelection"/> component on the same GameObject.
/// </summary>  
[RequireComponent(typeof(LocationSelection))]
public class Step2_LocationSelect : LevelCreationStep
{
    private LocationSelection locationSelection;

    private void Awake()
    {
        locationSelection = GetComponent<LocationSelection>();
    }

    /// <summary>
    /// Validates the step. Returns true if a location is selected.
    /// Saves the selected location to <see cref="GameSettings"/> if valid.
    /// </summary>
    /// <returns>True if a location is selected, false otherwise.</returns>
    public override bool ValidateStep()
    {
        LocationData location = locationSelection.GetSelectedLocation();
        if (location == null)
        {
            Debug.LogWarning("[Step2_LocationSelect] No location selected.");
            return false;
        }

        GameSettings.Instance.SelectedLocationId = location.locationId;
        return true;
    }
}

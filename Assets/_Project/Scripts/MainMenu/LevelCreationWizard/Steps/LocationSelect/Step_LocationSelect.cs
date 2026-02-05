using UnityEngine;
 
[RequireComponent(typeof(LocationSelection))]
public class Step_LocationSelect : BaseLevelCreationStep
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
            Debug.LogWarning("[Step_LocationSelect] No location selected.");
            return false;
        }

        GameSettings.Instance.CurrentEditorSession.SelectedLocationId = location.locationId;
        return true;
    }
}

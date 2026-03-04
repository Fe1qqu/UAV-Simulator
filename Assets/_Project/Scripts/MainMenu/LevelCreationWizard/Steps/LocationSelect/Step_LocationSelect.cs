using UnityEngine;
 
[RequireComponent(typeof(LocationSelection))]
public class Step_LocationSelect : LevelCreationWizardStepBase
{
    private LocationSelection locationSelection;

    protected override void OnInitialized()
    {
        if (locationSelection == null)
        {
            locationSelection = GetComponent<LocationSelection>();
        }

        locationSelection.SetDatabase(MainMenuContext.LocationsDatabase);
    }

    public override void OnStepShown()
    {
        locationSelection.BuildIfNeeded();
    }

    /// <summary>
    /// Validates the step. Returns true if a location is selected.
    /// Saves the selected location to <see cref="GameSettings"/> if valid.
    /// </summary>
    /// <returns>True if a location is selected, false otherwise.</returns>
    public override bool ValidateStep()
    {
        LocationDefinition location = locationSelection.GetSelectedLocation();
        if (location == null)
        {
            Debug.LogWarning("[Step_LocationSelect] No location selected.");
            return false;
        }

        GameSettings.Instance.CurrentEditorSession.SelectedLocationId = location.locationId;
        return true;
    }
}

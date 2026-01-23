using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

/// <summary>
/// Preloads localization table required by the level creation wizard.
/// </summary>
public class LocationLocalizationPreloader : MonoBehaviour
{
    [Tooltip("Name of the locations StringTable.")]
    public string tableName = "Locations";

    private AsyncOperationHandle<StringTable> handle;
    private bool loadedSuccessfully = false;
    private bool isCancelled = false;

    /// <summary>
    /// True when the table has been loaded successfully.
    /// </summary>
    public bool IsLoaded => loadedSuccessfully;

    /// <summary>
    /// Loads the configured StringTable asynchronously.
    /// </summary>
    public async Task Load()
    {
        if (IsLoaded)
        {
            return;
        }

        isCancelled = false;

        handle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
        await handle.Task;

        if (isCancelled)
        {
            // The user closed the wizard before the load was complete
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            handle = default;
            loadedSuccessfully = false;
            return;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedSuccessfully = true;
            Debug.Log($"[LocationLocalizationPreloader] Table '{tableName}' loaded.");
        }
        else
        {
            loadedSuccessfully = false;
            Debug.LogError($"[LocationLocalizationPreloader] Failed to load table '{tableName}'.");
        }
    }

    public void Unload()
    {
        isCancelled = true;

        if (!handle.IsValid())
        {
            return;
        }

        Debug.Log($"[LocationLocalizationPreloader] Unloading '{tableName}'.");
        Addressables.Release(handle);
        handle = default;
        loadedSuccessfully = false;
    }
}

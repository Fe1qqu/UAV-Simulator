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

    /// <summary>
    /// True when the table has been loaded successfully.
    /// </summary>
    public bool IsLoaded => loadedSuccessfully;

    /// <summary>
    /// Loads the configured StringTable asynchronously.
    /// </summary>
    public async Task Load()
    {
        // Avoid double-loading
        if (IsLoaded)
        {
            return;
        }

        handle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedSuccessfully = true;
            Debug.Log($"[LocationTablePreloader] Table '{tableName}' loaded.");
        }
        else
        {
            loadedSuccessfully = false;
            Debug.LogError($"[LocationTablePreloader] Failed to load table '{tableName}'.");
        }
    }

    public void Unload()
    {
        if (!handle.IsValid())
        {
            return;
        }

        Debug.Log($"[LocationTablePreloader] Unloading '{tableName}'.");
        Addressables.Release(handle);
        handle = default;
        loadedSuccessfully = false;
    }
}

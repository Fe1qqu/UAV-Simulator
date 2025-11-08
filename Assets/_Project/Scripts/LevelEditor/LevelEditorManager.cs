using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ItemOption
{
    public string name;
    public Sprite icon;
    public GameObject prefab;
}

public class LevelEditorManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown locationDropdown;
    public Transform objectListContainer;
    public GameObject objectListItemTemplate;
    public Transform levelRoot;

    [Header("Locations")]
    public List<ItemOption> locations;

    [Header("Placeable Objects")]
    public List<ItemOption> placeableObjects;

    private GameObject currentLocation;

    void Start()
    {
        if (locationDropdown == null)
        {
            Debug.LogError($"[LevelEditorManager] TMP_Dropdown is missing on '{gameObject.name}'.");
            return;
        }

        if (levelRoot == null)
        {
            Debug.LogError($"[LevelEditorManager] LevelRoot Transform is not assigned on '{gameObject.name}'.");
            return;
        }

        if (locations == null)
        {
            Debug.LogError($"[LevelEditorManager] Locations list is null on '{gameObject.name}'.");
            return;
        }

        if (locations.Count == 0)
        {
            Debug.LogError($"[LevelEditorManager] Locations list is empty on '{gameObject.name}'. Please assign at least one location in the inspector.");
            return;
        }

        SetupLocationDropdown();
        SetupObjectList();

        locationDropdown.onValueChanged.AddListener(OnLocationChanged);

        OnLocationChanged(0);
    }

    void SetupLocationDropdown()
    {
        locationDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < locations.Count; i++)
        {
            var location = locations[i];

            if (location == null)
            {
                Debug.LogError($"[LevelEditorManager] Location element at index {i} is null on '{gameObject.name}'.");
                continue;
            }

            if (location.prefab == null)
            {
                Debug.LogError($"[LevelEditorManager] Location '{location.name}' has no prefab assigned on '{gameObject.name}'.");
                continue;
            }

            options.Add(new TMP_Dropdown.OptionData(location.name, location.icon, Color.black));
        }

        locationDropdown.AddOptions(options);
    }

    void SetupObjectList()
    {
        if (objectListItemTemplate == null)
        {
            Debug.LogError($"[LevelEditorManager] Object List Item Template not assigned on '{gameObject.name}'.");
            return;
        }

        foreach (Transform child in objectListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var obj in placeableObjects)
        {
            if (obj.prefab == null)
            {
                Debug.LogError($"[LevelEditorManager] Placeable object '{obj.name}' missing prefab.");
                continue;
            }

            var item = Instantiate(objectListItemTemplate, objectListContainer);
            item.SetActive(true);

            var icon = item.transform.Find("Icon")?.GetComponent<Image>();
            var label = item.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            if (icon != null) icon.sprite = obj.icon;
            if (label != null) label.text = obj.name;

            var dragItem = item.GetComponent<UIObjectDraggableItem>();
            if (dragItem != null)
            {
                dragItem.linkedObject = obj;
            }
            else
            {
                Debug.LogError($"[LevelEditorManager] ObjectListItem '{item.name}' missing UIObjectDraggableItem component.");
            }
        }
    }

    public void OnLocationChanged(int index)
    {
        if (currentLocation != null)
        {
            Destroy(currentLocation);
        }

        if (index < 0 || index >= locations.Count)
        {
            Debug.LogError($"[LevelEditorManager] Invalid location index ({index}) on '{gameObject.name}'. Valid range: 0–{locations.Count - 1}");
            return;
        }

        currentLocation = Instantiate(locations[index].prefab, Vector3.zero, Quaternion.identity, levelRoot);
    }
}

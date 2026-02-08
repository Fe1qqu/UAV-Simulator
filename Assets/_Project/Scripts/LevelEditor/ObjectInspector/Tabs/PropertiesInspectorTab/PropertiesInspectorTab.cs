using UnityEngine;

public class PropertiesInspectorTab : MonoBehaviour
{
    [SerializeField] private Transform listRoot;
    [SerializeField] private PropertyInspectorField propertyInspectorFieldPrefab;

    private LevelObject boundObject;
    private bool isDirty;
    private bool isActive;

    private void Awake()
    {
        if (listRoot == null)
        {
            Debug.LogError("[PropertiesInspectorTab] ListRoot is not assigned.");
        }

        if (propertyInspectorFieldPrefab == null)
        {
            Debug.LogError("[PropertiesInspectorTab] PropertyInspectorFieldPrefab is not assigned.");
        }
    }

    public void Bind(LevelObject levelObject)
    {
        boundObject = levelObject;
        isDirty = true;

        if (isActive)
        {
            Rebuild();
        }
    }

    public void Clear()
    {
        boundObject = null;
        isDirty = false;
        ClearUI();
    }

    public void OnActivated()
    {
        isActive = true;

        if (isDirty)
        {
            Rebuild();
        }
    }

    public void OnDeactivated()
    {
        isActive = false;
    }

    private void Rebuild()
    {
        ClearUI();

        if (boundObject == null)
        {
            return;
        }

        foreach (LevelObjectProperty property in boundObject.Properties)
        {
            PropertyInspectorField propertyFieldInstance = Instantiate(propertyInspectorFieldPrefab, listRoot);
            propertyFieldInstance.gameObject.SetActive(true);
            propertyFieldInstance.Bind(boundObject, property.key);
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in listRoot)
        {
            Destroy(child.gameObject);
        }
    }
}

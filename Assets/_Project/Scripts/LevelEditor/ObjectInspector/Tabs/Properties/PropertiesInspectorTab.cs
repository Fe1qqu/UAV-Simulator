using UnityEngine;

public class PropertiesInspectorTab : MonoBehaviour
{
    [SerializeField] private Transform listRoot;

    [SerializeField] private PropertyInspectorField_Input inputFieldPrefab;
    [SerializeField] private PropertyInspectorField_Bool boolFieldPrefab;
    //[SerializeField] private PropertyInspectorField_Color colorFieldPrefab;
    [SerializeField] private PropertyInspectorField_Enum enumFieldPrefab;

    private LevelObject boundObject;
    private bool isDirty;
    private bool isActive;

    private void Awake()
    {
        if (listRoot == null)
        {
            Debug.LogError("[PropertiesInspectorTab] ListRoot is not assigned.");
        }

        if (inputFieldPrefab == null)
        {
            Debug.LogError("[PropertiesInspectorTab] InputFieldPrefab is not assigned.");
        }

        if (boolFieldPrefab == null)
        {
            Debug.LogError("[PropertiesInspectorTab] BoolFieldPrefab is not assigned.");
        }

        //if (colorFieldPrefab == null)
        //{
        //    Debug.LogError("[PropertiesInspectorTab] ColorFieldPrefab is not assigned.");
        //}

        if (enumFieldPrefab == null)
        {
            Debug.LogError("[PropertiesInspectorTab] EnumFieldPrefab is not assigned.");
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

        foreach (ObjectPropertyDefinition objectProperty in boundObject.SourcePlaceableObject.properties)
        {
            PropertyInspectorFieldBase propertyFieldInstance = CreateField(objectProperty);
            propertyFieldInstance.Bind(boundObject, objectProperty);
            propertyFieldInstance.gameObject.SetActive(true);
        }
    }

    private PropertyInspectorFieldBase CreateField(ObjectPropertyDefinition objectProperty)
    {
        switch (objectProperty.type)
        {
            case ObjectPropertyType.Int:
            case ObjectPropertyType.Float:
            case ObjectPropertyType.String:
                return Instantiate(inputFieldPrefab, listRoot);

            case ObjectPropertyType.Bool:
                return Instantiate(boolFieldPrefab, listRoot);

            case ObjectPropertyType.Enum:
                return Instantiate(enumFieldPrefab, listRoot);

            // case ObjectPropertyType.Color:
            //     return Instantiate(colorFieldPrefab, listRoot);

            default:
                return Instantiate(inputFieldPrefab, listRoot);
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

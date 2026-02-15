using UnityEngine;
using TMPro;
using System.Linq;

public class PropertyInspectorField : MonoBehaviour
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_InputField valueInputField;

    private LevelObject levelObject;
    private string propertyKey;
    private bool suppressNotify;

    private void Awake()
    {
        if (keyText == null)
        {
            Debug.LogError("[PropertyInspectorField] KeyText is not assigned.");
        }

        if (valueInputField == null)
        {
            Debug.LogError("[PropertyInspectorField] ValueInputField is not assigned.");
        }
    }

    public void Bind(LevelObject levelObject, string key)
    {
        this.levelObject = levelObject;
        propertyKey = key;

        LevelObjectProperty property = levelObject.Properties.FirstOrDefault(property => property.key == key);

        keyText.text = key;
        valueInputField.text = property.value;

        valueInputField.onEndEdit.RemoveAllListeners();
        valueInputField.onEndEdit.AddListener(OnValueChanged);

        levelObject.PropertyChanged += OnPropertyChanged;
    }

    private void OnDestroy()
    {
        if (levelObject != null)
        {
            levelObject.PropertyChanged -= OnPropertyChanged;
        }
    }

    private void OnValueChanged(string newValue)
    {
        if (suppressNotify)
        {
            return;
        }

        levelObject.TrySetProperty(propertyKey, newValue);
    }


    private void OnPropertyChanged(LevelObject levelObject, string changedKey)
    {
        if (changedKey != propertyKey)
        {
            return;
        }

        LevelObjectProperty property = levelObject.Properties.FirstOrDefault(property => property.key == propertyKey);

        suppressNotify = true;
        valueInputField.text = property.value;
        suppressNotify = false;
    }
}

using UnityEngine;
using TMPro;

public class Step_NameInput : LevelCreationWizardStepBase
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private UIValidationFeedback validationFeedback;

    private void Awake()
    {
        if (nameInputField == null)
        {
            Debug.LogError("[Step_NameInput] NameInputField is not assigned.");
        }

        if (validationFeedback == null)
        {
            Debug.LogError("[Step_NameInput] ValidationFeedback is not assigned.");
        }
    }

    private void OnEnable()
    {
        nameInputField.onValueChanged.AddListener(OnNameChanged);
    }

    private void OnDisable()
    {
        nameInputField.onValueChanged.RemoveListener(OnNameChanged);
    }

    /// <summary>
    /// Called whenever the input field value changes. Updates the color if valid.
    /// </summary>
    private void OnNameChanged(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            validationFeedback.SetNormal();
        }
    }

    public override bool ValidateStep()
    {
        string name = nameInputField.text.Trim();

        bool valid = !string.IsNullOrWhiteSpace(name);

        if (!valid)
        {
            validationFeedback.PlayErrorFlash();
            return false;
        }

        validationFeedback.SetNormal();

        LevelCreationWizard.Data.LevelName = name;

        return true;
    }
}

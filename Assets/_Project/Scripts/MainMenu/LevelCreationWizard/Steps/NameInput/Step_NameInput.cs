using UnityEngine;
using TMPro;

public class Step_NameInput : BaseLevelCreationStep
{
    [Header("UI References")]
    [Tooltip("Input field where the user enters the level name.")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Visual Settings")]
    [Tooltip("Input field color when the value is valid.")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Input field color when the value is invalid.")]
    [SerializeField] private Color errorColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);

    private void Awake()
    {
        if (nameInputField == null)
        {
            Debug.LogError("[Step_NameInput] nameInputField is not assigned.");
            return;
        }
    }

    private void Start()
    {
        nameInputField.onValueChanged.AddListener(OnNameChanged);
    }

    private void OnDestroy()
    {
        nameInputField.onValueChanged.RemoveListener(OnNameChanged);
    }

    /// <summary>
    /// Called whenever the input field value changes. Updates the color if valid.
    /// </summary>
    /// <param name="newText">New input text.</param>
    private void OnNameChanged(string newText)
    {
        bool valid = !string.IsNullOrEmpty(name);

        if (valid)
        {
            nameInputField.image.color = normalColor;
        }
    }

    /// <summary>
    /// Validates the step. Returns true if the level name is not empty.
    /// Saves the level name to <see cref="GameSettings"/> if valid.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public override bool ValidateStep()
    {
        string name = nameInputField.text.Trim();
        bool valid = !string.IsNullOrEmpty(name);

        nameInputField.image.color = valid ? normalColor : errorColor;

        if (valid)
        {
            EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;
            editorSession.LevelName = name;
        }

        return valid;
    }
}

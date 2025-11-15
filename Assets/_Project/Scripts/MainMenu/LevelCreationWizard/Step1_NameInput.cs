using UnityEngine;
using TMPro;


/// <summary>
/// Step 1 of the level creation wizard. Handles user input for naming the level.
/// </summary>
public class Step1_NameInput : LevelCreationStep
{
    [Header("UI References")]
    [Tooltip("Input field where the user enters the level name.")]
    public TMP_InputField nameInput;

    [Header("Visual Settings")]
    [Tooltip("Input field color when the value is valid.")]
    public Color normalColor = Color.white;

    [Tooltip("Input field color when the value is invalid.")]
    public Color errorColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);

    private void Awake()
    {
        if (nameInput == null)
        {
            Debug.LogError("[Step1_NameInput] nameInput is not assigned.");
            return;
        }

        nameInput.onValueChanged.AddListener(OnNameChanged);
    }

    private void OnDestroy()
    {
        if (nameInput == null)
        {
            return;
        }

        nameInput.onValueChanged.RemoveListener(OnNameChanged);
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
            nameInput.image.color = normalColor;
        }
    }

    /// <summary>
    /// Validates the step. Returns true if the level name is not empty.
    /// Saves the level name to <see cref="GameSettings"/> if valid.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public override bool ValidateStep()
    {
        string name = nameInput.text.Trim();
        bool valid = !string.IsNullOrEmpty(name);

        nameInput.image.color = valid ? normalColor : errorColor;

        if (valid)
        {
            GameSettings.Instance.LevelName = name;
        }

        return valid;
    }
}

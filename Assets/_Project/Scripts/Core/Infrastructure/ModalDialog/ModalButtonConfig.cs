using System;

[Serializable]
public class ModalButtonConfig
{
    /// <summary>
    /// Localization table entry key
    /// </summary>
    public string TextKey;

    public ModalResult Result;

    /// <summary>
    /// If true, this button will be pressed when Back is pressed while the modal window is open.
    /// Only one button should have this flag per modal window.
    /// </summary>
    public bool IsBackAction;
}

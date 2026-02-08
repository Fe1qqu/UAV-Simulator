using System;
using System.Collections.Generic;

public class ModalDialogConfig
{
    /// <summary>
    /// Localization table entry key
    /// </summary>
    public string MessageKey;

    public IReadOnlyList<ModalButtonConfig> Buttons;

    public Action<ModalResult> OnResult;
}

using System;
using System.Collections.Generic;
using TMPro;

public class ModalDialogConfig
{
    // Title (optional)
    public string titleLocalizationKey;
    public object[] titleArguments;

    // Body message (optional single line or base text)
    public string messageLocalizationKey;
    public object[] messageArguments;

    // Validation / list block (optional)
    public IReadOnlyList<string> messageLines;

    // UI
    public TextAlignmentOptions messageAlignment = TextAlignmentOptions.Center;

    public IReadOnlyList<ModalButtonConfig> Buttons;

    public Action<ModalResult> OnResult;
}

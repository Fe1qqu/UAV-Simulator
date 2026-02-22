using System;
using System.Collections.Generic;

public class ModalDialogConfig
{
    public string messageLocalizationTableEntryKey;

    public IReadOnlyList<ModalButtonConfig> Buttons;

    public Action<ModalResult> OnResult;
}

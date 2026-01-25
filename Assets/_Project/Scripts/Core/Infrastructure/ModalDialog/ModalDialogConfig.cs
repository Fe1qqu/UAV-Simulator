using System;
using System.Collections.Generic;

public class ModalDialogConfig
{
    public string Message;

    public IReadOnlyList<ModalButtonConfig> Buttons;

    public Action<ModalResult> OnResult;
}

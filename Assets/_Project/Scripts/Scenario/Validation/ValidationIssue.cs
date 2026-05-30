using System;

public readonly struct ValidationIssue
{
    public readonly ScenarioValidationErrorType ErrorType;
    public readonly string LocalizationKey;
    public readonly object[] Arguments;

    public ValidationIssue(
        ScenarioValidationErrorType errorType,
        string localizationKey,
        params object[] arguments)
    {
        ErrorType = errorType;
        LocalizationKey = localizationKey;
        Arguments = arguments;
    }
}

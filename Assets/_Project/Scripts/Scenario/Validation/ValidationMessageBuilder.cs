using UnityEngine.Localization;
using System.Collections.Generic;

public static class ValidationMessageBuilder
{
    public static List<string> Build(ScenarioValidationResult result)
    {
        List<string> lines = new();

        foreach (ValidationIssue issue in result.Issues)
        {
            LocalizedString localized = new("UI", issue.LocalizationKey)
            {
                Arguments = issue.Arguments
            };

            lines.Add(localized.GetLocalizedString());
        }

        return lines;
    }
}

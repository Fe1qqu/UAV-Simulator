using System.Collections.Generic;
using System.Linq;

public sealed class ScenarioValidationResult
{
    private readonly List<ValidationIssue> issues;

    public bool IsValid => issues.Count == 0;

    public IReadOnlyList<ValidationIssue> Issues => issues;

    public ScenarioValidationResult()
    {
        issues = new List<ValidationIssue>();
    }

    public ScenarioValidationResult(IEnumerable<ValidationIssue> issues)
    {
        this.issues = issues?.ToList() ?? new List<ValidationIssue>();
    }

    public void Add(ValidationIssue issue)
    {
        issues.Add(issue);
    }

    public void AddRange(IEnumerable<ValidationIssue> issues)
    {
        this.issues.AddRange(issues);
    }

    public static ScenarioValidationResult Ok()
    {
        return new ScenarioValidationResult();
    }
}

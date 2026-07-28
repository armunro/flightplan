using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models;
using FlightPlan.Models.Config;
using FlightPlan.Services;

namespace FlightPlan.Infrastructure.Services;

public class MockRuleService : IRuleService
{
    private readonly IEmailService _emailService;
    private readonly DashConfig _config;
    private readonly List<FilterRule> _rules = new();

    public MockRuleService(IEmailService emailService, DashConfig config)
    {
        _emailService = emailService;
        _config = config;
        AddMockRules();
    }

    private void AddMockRules()
    {
        var mockRules = new List<FilterRule>
        {
            new FilterRule
            {
                Name = "Jira Notifications",
                Color = "#0052CC",
                Filters = new List<FilterCriteria>
                {
                    new FilterCriteria { From = new List<string> { "jira@example.com" } }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction { Type = ActionType.AddCategory, Value = "Jira" }
                }
            },
            new FilterRule
            {
                Name = "GitHub Activity",
                Color = "#24292E",
                Filters = new List<FilterCriteria>
                {
                    new FilterCriteria { From = new List<string> { "noreply@github.com" } }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction { Type = ActionType.AddCategory, Value = "GitHub" }
                }
            },
            new FilterRule
            {
                Name = "Azure Alerts",
                Color = "#0089D6",
                Filters = new List<FilterCriteria>
                {
                    new FilterCriteria { SubjectContains = new List<string> { "Azure Alerts", "High CPU usage" } }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction { Type = ActionType.AddCategory, Value = "Infrastructure" },
                    new RuleAction { Type = ActionType.Star }
                }
            },
            new FilterRule
            {
                Name = "Security Reports",
                Color = "#D13438",
                Filters = new List<FilterCriteria>
                {
                    new FilterCriteria { SubjectContains = new List<string> { "Security Report" } }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction { Type = ActionType.MarkAsRead }
                }
            }
        };

        _rules.AddRange(mockRules);
    }

    public List<FilterRule> GetAllRules() => _rules;

    public Task SaveRuleAsync(FilterRule rule)
    {
        var existing = _rules.FirstOrDefault(r => r.Name.Equals(rule.Name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            _rules.Remove(existing);
        }
        _rules.Add(rule);
        return Task.CompletedTask;
    }

    public Task DeleteRuleAsync(string ruleName)
    {
        var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (rule != null)
        {
            _rules.Remove(rule);
        }
        return Task.CompletedTask;
    }

    public Task<FilterRule> AddSenderToRuleAsync(string ruleName, string senderEmail, string subject)
    {
        var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (rule == null) throw new ArgumentException($"Rule '{ruleName}' not found.");

        if (rule.Filters == null) rule.Filters = new List<FilterCriteria>();
        
        var criteria = new FilterCriteria 
        { 
            From = new List<string> { senderEmail },
            SubjectContains = string.IsNullOrEmpty(subject) ? new List<string>() : new List<string> { subject }
        };
        
        rule.Filters.Add(criteria);
        return Task.FromResult(rule);
    }

    public Task<FilterRule> CreateRuleFromEmailAsync(string messageId, string? ruleName = null)
    {
        var rule = new FilterRule
        {
            Name = ruleName ?? "New Mock Rule",
            Color = "#3498db",
            RootFolder = "Inbox",
            Filters = new List<FilterCriteria>
            {
                new FilterCriteria
                {
                    From = new List<string> { "sender@example.com" }
                }
            },
            Actions = new List<RuleAction>
            {
                new RuleAction { Type = ActionType.MarkAsRead }
            }
        };

        _rules.Add(rule);
        return Task.FromResult(rule);
    }

    public async Task ApplyRuleAsync(string messageId, string ruleName)
    {
        var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (rule == null) throw new ArgumentException($"Rule '{ruleName}' not found.");

        await _emailService.ApplyRuleActionsAsync(messageId, rule.Actions);
    }

    public bool Matches(FilterRule rule, EmailDto email)
    {
        if (rule.Filters == null || !rule.Filters.Any())
            return true;

        return rule.Filters.Any(criteria => MatchesCriteria(criteria, email));
    }

    private bool MatchesCriteria(FilterCriteria criteria, EmailDto email)
    {
        if (criteria.From != null && criteria.From.Any())
        {
            if (!criteria.From.Any(f => 
                email.From.Contains(f, StringComparison.OrdinalIgnoreCase) || 
                email.FromAddress.Contains(f, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        if (criteria.SubjectContains != null && criteria.SubjectContains.Any())
        {
            if (!criteria.SubjectContains.Any(s => email.Subject.Contains(s, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        if (criteria.BodyContains != null && criteria.BodyContains.Any())
        {
            if (!criteria.BodyContains.Any(b => email.BodyPreview.Contains(b, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        return true;
    }
}

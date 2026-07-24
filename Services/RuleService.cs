using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlightPlan.Services;

public interface IRuleService
{
    List<FilterRule> GetAllRules();
    Task ApplyRuleAsync(string messageId, string ruleName);
    bool Matches(FilterRule rule, EmailDto email);
    Task<FilterRule> CreateRuleFromEmailAsync(string messageId, string? ruleName = null);
    Task SaveRuleAsync(FilterRule rule);
    Task DeleteRuleAsync(string ruleName);
    Task<FilterRule> AddSenderToRuleAsync(string ruleName, string senderEmail, string subject);
}

public class RuleService : IRuleService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<RuleService> _logger;
    private readonly IStorageService _storageService;
    private readonly List<FilterRule> _rules = new();
    private readonly string _rulesDirectory;

    public RuleService(IEmailService emailService, ILogger<RuleService> logger, IStorageService storageService)
    {
        _emailService = emailService;
        _logger = logger;
        _storageService = storageService;
        _rulesDirectory = _storageService.GetRulesDirectory();
        MigrateRules();
        LoadRules();
    }

    private void MigrateRules()
    {
        var oldRulesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rules");
        if (Directory.Exists(oldRulesDir) && oldRulesDir != _rulesDirectory)
        {
            foreach (var file in Directory.GetFiles(oldRulesDir, "*.yaml"))
            {
                var destFile = Path.Combine(_rulesDirectory, Path.GetFileName(file));
                if (!File.Exists(destFile))
                {
                    try { File.Copy(file, destFile); } catch { }
                }
            }
        }
    }

    private void LoadRules()
    {
        if (!Directory.Exists(_rulesDirectory)) return;

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var files = Directory.GetFiles(_rulesDirectory, "*.yaml");
        foreach (var file in files)
        {
            try
            {
                var yaml = File.ReadAllText(file);
                var rule = deserializer.Deserialize<FilterRule>(yaml);
                if (rule != null)
                {
                    if (string.IsNullOrEmpty(rule.Name)) rule.Name = Path.GetFileNameWithoutExtension(file);
                    _rules.Add(rule);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rule from {File}", file);
            }
        }
    }

    public List<FilterRule> GetAllRules() => _rules;

    public async Task SaveRuleAsync(FilterRule rule)
    {
        // Handle renaming: if OriginalName is set and different from Name, delete the old rule file
        if (!string.IsNullOrEmpty(rule.OriginalName) && !rule.OriginalName.Equals(rule.Name, StringComparison.OrdinalIgnoreCase))
        {
            var oldFileName = GetSafeFileName(rule.OriginalName);
            var oldFilePath = Path.Combine(_rulesDirectory, oldFileName);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }

            // Remove the old rule from the in-memory list
            var oldRule = _rules.FirstOrDefault(r => r.Name.Equals(rule.OriginalName, StringComparison.OrdinalIgnoreCase));
            if (oldRule != null)
            {
                _rules.Remove(oldRule);
            }
        }

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Don't persist OriginalName to YAML
        var originalNameBackup = rule.OriginalName;
        rule.OriginalName = null;
        var yaml = serializer.Serialize(rule);
        rule.OriginalName = originalNameBackup;

        var fileName = GetSafeFileName(rule.Name);
        var filePath = Path.Combine(_rulesDirectory, fileName);

        if (!Directory.Exists(_rulesDirectory))
        {
            Directory.CreateDirectory(_rulesDirectory);
        }

        await File.WriteAllTextAsync(filePath, yaml);

        // Update in-memory list
        var existing = _rules.FirstOrDefault(r => r.Name.Equals(rule.Name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            _rules.Remove(existing);
        }
        _rules.Add(rule);
    }

    public async Task<FilterRule> AddSenderToRuleAsync(string ruleName, string senderEmail, string subject)
    {
        var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (rule == null) throw new ArgumentException($"Rule '{ruleName}' not found.");

        if (rule.Filters == null) rule.Filters = new List<FilterCriteria>();
        
        // Create a new criteria instead of adding to existing one
        var criteria = new FilterCriteria 
        { 
            From = new List<string> { senderEmail },
            SubjectContains = string.IsNullOrEmpty(subject) ? new List<string>() : new List<string> { subject }
        };
        
        rule.Filters.Add(criteria);
        await SaveRuleAsync(rule);
        return rule;
    }

    public async Task DeleteRuleAsync(string ruleName)
    {
        var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (rule == null) return;

        var fileName = GetSafeFileName(rule.Name);
        var filePath = Path.Combine(_rulesDirectory, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _rules.Remove(rule);
    }

    private string GetSafeFileName(string ruleName)
    {
        var fileName = $"{ruleName.Replace(" ", "-")}.yaml";
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    public async Task<FilterRule> CreateRuleFromEmailAsync(string messageId, string? ruleName = null)
    {
        var email = await _emailService.GetEmailAsync(messageId);
        if (email == null) throw new ArgumentException("Email not found");

        if (string.IsNullOrEmpty(ruleName))
        {
            ruleName = email.From;
        }

        var rule = new FilterRule
        {
            Name = ruleName,
            Color = "#3498db", // Default color
            RootFolder = "Inbox",
            Filters = new List<FilterCriteria>
            {
                new FilterCriteria
                {
                    From = new List<string> { email.FromAddress }
                }
            },
            Actions = new List<RuleAction>
            {
                new RuleAction { Type = ActionType.MarkAsRead },
                new RuleAction { Type = ActionType.Move, Value = "Archive" }
            }
        };

        if (!Directory.Exists(_rulesDirectory))
        {
            Directory.CreateDirectory(_rulesDirectory);
        }

        await SaveRuleAsync(rule);
        return rule;
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
            return true; // Match everything if no filters

        return rule.Filters.Any(criteria => MatchesCriteria(criteria, email));
    }

    private bool MatchesCriteria(FilterCriteria criteria, EmailDto email)
    {
        // From
        if (criteria.From != null && criteria.From.Any())
        {
            if (!criteria.From.Any(f => 
                email.From.Contains(f, StringComparison.OrdinalIgnoreCase) || 
                email.FromAddress.Contains(f, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Subject
        if (criteria.SubjectContains != null && criteria.SubjectContains.Any())
        {
            if (!criteria.SubjectContains.Any(s => email.Subject.Contains(s, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // Body
        if (criteria.BodyContains != null && criteria.BodyContains.Any())
        {
            if (!criteria.BodyContains.Any(b => email.BodyPreview.Contains(b, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        return true;
    }
}

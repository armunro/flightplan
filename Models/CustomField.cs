namespace FlightPlan.Models;

public enum CustomFieldType
{
    Text,
    SingleSelect,
    MultiSelect
}

public class CustomFieldDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CustomFieldType Type { get; set; }
    public List<string> Options { get; set; } = new();
}

public class CustomFieldValue
{
    public Guid DefinitionId { get; set; }
    public string? Value { get; set; } // For Text and SingleSelect
    public List<string> Values { get; set; } = new(); // For MultiSelect
}

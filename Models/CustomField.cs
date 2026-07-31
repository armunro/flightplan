namespace FlightPlan.Models;

public enum CustomFieldType
{
    Text,
    SingleSelect,
    MultiSelect,
    Date,
    Link,
    Money,
    Boolean
}

public class CustomFieldOption
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class CustomFieldDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CustomFieldType Type { get; set; }
    public List<CustomFieldOption> Options { get; set; } = new();
}

public class CustomFieldValue
{
    public Guid DefinitionId { get; set; }
    public string? Value { get; set; } // For Text and SingleSelect
    public List<string> Values { get; set; } = new(); // For MultiSelect
}

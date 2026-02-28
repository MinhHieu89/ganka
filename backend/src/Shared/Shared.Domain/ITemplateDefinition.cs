namespace Shared.Domain;

/// <summary>
/// Interface for disease-specific clinical templates that can be added without code changes.
/// Templates define the structure of clinical data collection forms.
///
/// Concrete implementations:
/// - Dry Eye template (Phase 4)
/// - Myopia Control template (post-launch)
/// - Keratoconus template (post-launch)
///
/// Templates are loaded from configuration/database, enabling clinics to customize
/// data collection fields per disease type.
/// </summary>
public interface ITemplateDefinition
{
    /// <summary>
    /// Unique template identifier (e.g., "dry-eye-v1", "myopia-control-v1").
    /// </summary>
    string TemplateId { get; }

    /// <summary>
    /// Human-readable template name (e.g., "Dry Eye Assessment", "Myopia Control Protocol").
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    /// Template version for schema evolution tracking.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Ordered list of fields that make up this template.
    /// </summary>
    IReadOnlyList<TemplateField> Fields { get; }
}

/// <summary>
/// Defines a single field within a clinical template.
/// </summary>
public sealed record TemplateField
{
    /// <summary>
    /// Field name used as the data key (e.g., "tbut_od", "osdi_score").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Data type for validation and UI rendering (e.g., "decimal", "integer", "string", "boolean", "select").
    /// </summary>
    public required string DataType { get; init; }

    /// <summary>
    /// Whether this field must be filled to complete the template.
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// JSON-encoded validation rules (e.g., {"min": 0, "max": 100, "unit": "seconds"}).
    /// </summary>
    public string ValidationRules { get; init; } = "{}";

    /// <summary>
    /// Order in which this field appears in the form UI.
    /// </summary>
    public required int DisplayOrder { get; init; }
}

/// <summary>
/// Registry for managing clinical template definitions.
/// Templates can be registered at startup or dynamically loaded from the database.
/// </summary>
public interface ITemplateRegistry
{
    /// <summary>
    /// Register a template definition for use in the system.
    /// </summary>
    void RegisterTemplate(ITemplateDefinition template);

    /// <summary>
    /// Retrieve a template by its unique identifier.
    /// </summary>
    /// <returns>The template definition, or null if not found.</returns>
    ITemplateDefinition? GetTemplate(string templateId);

    /// <summary>
    /// Get all registered template definitions.
    /// </summary>
    IReadOnlyList<ITemplateDefinition> GetAllTemplates();
}

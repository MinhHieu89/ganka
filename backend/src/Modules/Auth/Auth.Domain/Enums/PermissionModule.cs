namespace Auth.Domain.Enums;

/// <summary>
/// Represents functional modules in the system for permission scoping.
/// Each module maps to a bounded context in the modular monolith.
/// </summary>
public enum PermissionModule
{
    Auth,
    Patient,
    Clinical,
    Scheduling,
    Pharmacy,
    Optical,
    Billing,
    Treatment,
    Audit,
    Settings
}

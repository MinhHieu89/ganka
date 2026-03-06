using ContractEnums = Patient.Contracts.Enums;
using DomainEnums = Patient.Domain.Enums;

namespace Patient.Application.Mappers;

/// <summary>
/// Extension methods for type-safe enum conversion between Domain and Contracts layers.
/// Uses int cast since enum values are intentionally identical.
/// </summary>
public static class EnumMappers
{
    // Gender
    public static ContractEnums.Gender ToContractEnum(this DomainEnums.Gender value)
        => (ContractEnums.Gender)(int)value;
    public static DomainEnums.Gender ToDomainEnum(this ContractEnums.Gender value)
        => (DomainEnums.Gender)(int)value;

    // PatientType
    public static ContractEnums.PatientType ToContractEnum(this DomainEnums.PatientType value)
        => (ContractEnums.PatientType)(int)value;
    public static DomainEnums.PatientType ToDomainEnum(this ContractEnums.PatientType value)
        => (DomainEnums.PatientType)(int)value;

    // AllergySeverity
    public static ContractEnums.AllergySeverity ToContractEnum(this DomainEnums.AllergySeverity value)
        => (ContractEnums.AllergySeverity)(int)value;
    public static DomainEnums.AllergySeverity ToDomainEnum(this ContractEnums.AllergySeverity value)
        => (DomainEnums.AllergySeverity)(int)value;
}

using FluentAssertions;
using Patient.Domain.Enums;
using Patient.Domain.Services;

namespace Patient.Unit.Tests;

/// <summary>
/// TDD tests for PatientFieldValidator domain service.
/// Tests validate that Address and CCCD are enforced by context:
/// - Registration: fields are optional (no errors)
/// - Referral/LegalExport: fields are required (errors when missing)
/// </summary>
public class PatientFieldValidatorTests
{
    [Fact]
    public void Validate_Registration_NullAddressAndCccd_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate(null, null, FieldRequirementContext.Registration);

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Referral_NullAddress_ReturnsAddressRequired()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate(null, "012345678901", FieldRequirementContext.Referral);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingFields.Should().ContainSingle(f => f.FieldName == "Address");
    }

    [Fact]
    public void Validate_Referral_NullCccd_ReturnsCccdRequired()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate("123 Main St", null, FieldRequirementContext.Referral);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingFields.Should().ContainSingle(f => f.FieldName == "Cccd");
    }

    [Fact]
    public void Validate_Referral_BothPresent_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate("123 Main St", "012345678901", FieldRequirementContext.Referral);

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void Validate_LegalExport_NullAddress_ReturnsAddressRequired()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate(null, "012345678901", FieldRequirementContext.LegalExport);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingFields.Should().ContainSingle(f => f.FieldName == "Address");
    }

    [Fact]
    public void Validate_LegalExport_NullCccd_ReturnsCccdRequired()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate("123 Main St", null, FieldRequirementContext.LegalExport);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingFields.Should().ContainSingle(f => f.FieldName == "Cccd");
    }

    [Fact]
    public void Validate_LegalExport_BothPresent_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate("123 Main St", "012345678901", FieldRequirementContext.LegalExport);

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Registration_BothPresent_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = PatientFieldValidator.Validate("123 Main St", "012345678901", FieldRequirementContext.Registration);

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingFields.Should().BeEmpty();
    }
}

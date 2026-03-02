using FluentAssertions;
using Shared.Domain;

namespace Shared.Unit.Tests;

/// <summary>
/// Tests for the Error record, specifically the new ValidationWithDetails factory method
/// that carries structured field-level validation errors.
/// </summary>
public class ErrorTests
{
    [Fact]
    public void ValidationWithDetails_SetsCodeToValidation()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Full name is required." } },
            { "Phone", new[] { "Phone must be a valid Vietnamese phone number." } }
        };

        // Act
        var error = Error.ValidationWithDetails(errors);

        // Assert
        error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public void ValidationWithDetails_CarriesErrorsDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Full name is required." } },
            { "Phone", new[] { "Phone must be a valid Vietnamese phone number." } }
        };

        // Act
        var error = Error.ValidationWithDetails(errors);

        // Assert
        error.ValidationErrors.Should().NotBeNull();
        error.ValidationErrors.Should().HaveCount(2);
        error.ValidationErrors!["FullName"].Should().Contain("Full name is required.");
        error.ValidationErrors!["Phone"].Should().Contain("Phone must be a valid Vietnamese phone number.");
    }

    [Fact]
    public void ValidationWithDetails_MultipleMessagesPerField()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Phone", new[] { "Phone is required.", "Phone must be valid." } }
        };

        // Act
        var error = Error.ValidationWithDetails(errors);

        // Assert
        error.ValidationErrors!["Phone"].Should().HaveCount(2);
    }

    [Fact]
    public void Validation_String_HasNullValidationErrors()
    {
        // Arrange & Act - existing factory method should have null ValidationErrors
        var error = Error.Validation("Some error");

        // Assert
        error.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void Existing_Error_Methods_StillWork()
    {
        // Existing error creation should be unaffected
        var conflict = Error.Conflict("duplicate");
        conflict.Code.Should().Be("Error.Conflict");
        conflict.ValidationErrors.Should().BeNull();

        var notFound = Error.NotFound("Patient", Guid.NewGuid());
        notFound.Code.Should().Be("Error.NotFound");
        notFound.ValidationErrors.Should().BeNull();
    }
}

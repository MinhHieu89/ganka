using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain;
using Shared.Presentation;

namespace Shared.Unit.Tests;

/// <summary>
/// Tests for ResultExtensions verifying that structured validation errors
/// map to ProblemHttpResult with HttpValidationProblemDetails containing an "errors" dictionary.
/// Note: In .NET 10, Results.ValidationProblem returns ProblemHttpResult (not a separate ValidationProblem type).
/// The ProblemDetails is of type HttpValidationProblemDetails which has an Errors property.
/// </summary>
public class ResultExtensionsTests
{
    [Fact]
    public void ErrorPreservesValidationErrorsThroughResult()
    {
        // Verify Error.ValidationErrors survives Result.Failure round-trip
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Full name is required." } }
        };
        var error = Error.ValidationWithDetails(errors);
        error.ValidationErrors.Should().NotBeNull("error created with ValidationWithDetails");

        var result = Result<Guid>.Failure(error);
        result.Error.ValidationErrors.Should().NotBeNull("error passed through Result.Failure");
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static HttpValidationProblemDetails ExtractValidationProblemDetails(IResult httpResult)
    {
        httpResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = (ProblemHttpResult)httpResult;
        problemResult.StatusCode.Should().Be(400);
        problemResult.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        return (HttpValidationProblemDetails)problemResult.ProblemDetails;
    }

    [Fact]
    public void ToHttpResult_ValidationWithDetails_ReturnsValidationProblemWithErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Full name is required." } },
            { "Phone", new[] { "Phone must be a valid Vietnamese phone number." } }
        };
        var error = Error.ValidationWithDetails(errors);
        var result = Result<Guid>.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert - ProblemHttpResult with HttpValidationProblemDetails containing field-level errors
        var validationDetails = ExtractValidationProblemDetails(httpResult);
        validationDetails.Errors.Should().ContainKey("FullName");
        validationDetails.Errors.Should().ContainKey("Phone");
        validationDetails.Errors["FullName"].Should().Contain("Full name is required.");
        validationDetails.Errors["Phone"].Should().Contain("Phone must be a valid Vietnamese phone number.");
    }

    [Fact]
    public void ToHttpResult_ValidationStringError_ReturnsProblemWithoutValidationDetails()
    {
        // Arrange - existing string-based validation should still work
        var error = Error.Validation("Something went wrong");
        var result = Result<Guid>.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert - ProblemHttpResult but NOT HttpValidationProblemDetails
        httpResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = (ProblemHttpResult)httpResult;
        problemResult.ProblemDetails.Should().NotBeOfType<HttpValidationProblemDetails>(
            "string-based validation errors should use regular ProblemDetails, not HttpValidationProblemDetails");
    }

    [Fact]
    public void ToHttpResult_NonGenericResult_ValidationWithDetails_ReturnsValidationProblem()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required." } }
        };
        var error = Error.ValidationWithDetails(errors);
        var result = Result.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var validationDetails = ExtractValidationProblemDetails(httpResult);
        validationDetails.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public void ToCreatedHttpResult_ValidationWithDetails_ReturnsValidationProblem()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Required." } }
        };
        var error = Error.ValidationWithDetails(errors);
        var result = Result<Guid>.Failure(error);

        // Act
        var httpResult = result.ToCreatedHttpResult("/api/patients");

        // Assert
        var validationDetails = ExtractValidationProblemDetails(httpResult);
        validationDetails.Errors.Should().ContainKey("FullName");
    }
}

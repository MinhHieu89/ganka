using Microsoft.AspNetCore.Http;
using Shared.Domain;

namespace Shared.Presentation;

/// <summary>
/// Extension methods for mapping Result/Result&lt;T&gt; to IResult HTTP responses.
/// Centralizes error-code-to-HTTP-status mapping for all Presentation layer endpoints.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a non-generic Result to an IResult HTTP response.
    /// Success returns 200 OK; failure maps error codes to appropriate HTTP status codes.
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok();

        return MapError(result.Error);
    }

    /// <summary>
    /// Maps a generic Result&lt;T&gt; to an IResult HTTP response.
    /// Success returns 200 OK with the value; failure maps error codes to appropriate HTTP status codes.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return MapError(result.Error);
    }

    /// <summary>
    /// Maps a generic Result&lt;T&gt; to a 201 Created IResult HTTP response.
    /// Success returns 201 Created with location header; failure maps error codes to appropriate HTTP status codes.
    /// Used for POST endpoints that create resources (e.g., CreateUser, CreateRole).
    /// </summary>
    public static IResult ToCreatedHttpResult<T>(this Result<T> result, string routePrefix)
    {
        if (result.IsSuccess)
            return Results.Created($"{routePrefix}/{result.Value}", new { Id = result.Value });

        return MapError(result.Error);
    }

    private static IResult MapError(Error error)
    {
        return error.Code switch
        {
            "Error.Unauthorized" => Results.Problem(
                detail: error.Description,
                title: "Unauthorized",
                statusCode: 401),
            "Error.NotFound" => Results.NotFound(),
            "Error.Conflict" => Results.Conflict(new { error = error.Description }),
            "Error.Validation" => Results.Problem(
                detail: error.Description,
                title: "Validation Error",
                statusCode: 400),
            _ => Results.Problem(error.Description, statusCode: 400)
        };
    }
}

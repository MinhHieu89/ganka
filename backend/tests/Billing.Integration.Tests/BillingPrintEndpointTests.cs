using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Billing.Integration.Tests;

/// <summary>
/// Integration tests for PRT-03: Invoice and Receipt print endpoints.
/// Verifies that GET /api/billing/print/{invoiceId}/invoice and /receipt
/// return HTTP 200 with application/pdf content type and non-empty body.
/// </summary>
public class BillingPrintEndpointTests : IClassFixture<BillingWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly BillingWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BillingPrintEndpointTests(BillingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
    }

    // -----------------------------------------------------------------------
    // Helper: Login and return access token for authenticated requests
    // -----------------------------------------------------------------------
    private async Task<string> GetAccessTokenAsync()
    {
        var loginPayload = new
        {
            email = BillingWebApplicationFactory.TestUserEmail,
            password = BillingWebApplicationFactory.TestUserPassword
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "login should succeed");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = body.GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrEmpty("login should return an access token");
        return token!;
    }

    // -----------------------------------------------------------------------
    // Helper: Create authenticated request with Bearer token
    // -----------------------------------------------------------------------
    private async Task<HttpRequestMessage> CreateAuthenticatedRequest(HttpMethod method, string url)
    {
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Authorization", $"Bearer {token}");
        return request;
    }

    // -----------------------------------------------------------------------
    // Test 1: Invoice print endpoint returns 200 + PDF for valid invoice
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PrintInvoice_ValidInvoice_Returns200WithPdf()
    {
        // Arrange
        var invoiceId = _factory.SeededInvoiceId;
        var request = await CreateAuthenticatedRequest(HttpMethod.Get,
            $"/api/billing/print/{invoiceId}/invoice");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "invoice print endpoint should return 200 for a valid finalized invoice");

        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/pdf",
                "invoice print should return application/pdf content type");

        var body = await response.Content.ReadAsByteArrayAsync();
        body.Length.Should().BeGreaterThan(0,
            "invoice PDF response body should not be empty");
    }

    // -----------------------------------------------------------------------
    // Test 2: Receipt print endpoint returns 200 + PDF for valid invoice
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PrintReceipt_ValidInvoice_Returns200WithPdf()
    {
        // Arrange
        var invoiceId = _factory.SeededInvoiceId;
        var request = await CreateAuthenticatedRequest(HttpMethod.Get,
            $"/api/billing/print/{invoiceId}/receipt");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "receipt print endpoint should return 200 for a valid finalized invoice");

        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/pdf",
                "receipt print should return application/pdf content type");

        var body = await response.Content.ReadAsByteArrayAsync();
        body.Length.Should().BeGreaterThan(0,
            "receipt PDF response body should not be empty");
    }

    // -----------------------------------------------------------------------
    // Test 3: Invoice print for non-existent invoice returns error
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PrintInvoice_NonExistentInvoice_ReturnsError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = await CreateAuthenticatedRequest(HttpMethod.Get,
            $"/api/billing/print/{nonExistentId}/invoice");

        // Act
        var response = await _client.SendAsync(request);

        // Assert -- should return 500 (InvalidOperationException from service) or 404
        response.IsSuccessStatusCode.Should().BeFalse(
            "non-existent invoice should return error status");
    }

    // -----------------------------------------------------------------------
    // Test 4: Receipt print for non-existent invoice returns error
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PrintReceipt_NonExistentInvoice_ReturnsError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = await CreateAuthenticatedRequest(HttpMethod.Get,
            $"/api/billing/print/{nonExistentId}/receipt");

        // Act
        var response = await _client.SendAsync(request);

        // Assert -- should return 500 (InvalidOperationException from service) or 404
        response.IsSuccessStatusCode.Should().BeFalse(
            "non-existent invoice should return error status");
    }
}

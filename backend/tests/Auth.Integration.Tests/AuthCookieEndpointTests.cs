using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Net.Http.Headers;

namespace Auth.Integration.Tests;

/// <summary>
/// Integration tests for HTTP-only cookie refresh token storage.
/// Tests verify that login/refresh/logout endpoints handle refresh tokens
/// via Set-Cookie headers instead of JSON response bodies.
/// </summary>
public class AuthCookieEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthCookieEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        // Use the test server's handler directly -- no cookie container so we can inspect raw Set-Cookie headers
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
    }

    // -----------------------------------------------------------------------
    // Test 1: Login sets HttpOnly refresh token cookie
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Login_Success_SetsRefreshTokenCookie()
    {
        // Arrange
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Should have Set-Cookie header with refresh_token
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue("login should set a cookie");
        var setCookieHeader = cookies!.FirstOrDefault(c => c.Contains("refresh_token"));
        setCookieHeader.Should().NotBeNull("login should set a refresh_token cookie");

        // Cookie must be HttpOnly
        setCookieHeader.Should().Contain("httponly", "refresh_token cookie must be HttpOnly");

        // Cookie must be SameSite=Lax
        setCookieHeader.Should().Contain("samesite=lax", "refresh_token cookie must be SameSite=Lax");

        // Cookie must have Path=/api/auth
        setCookieHeader.Should().Contain("path=/api/auth", "refresh_token cookie must be scoped to /api/auth");

        // JSON body should contain accessToken, expiresAt, user
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("accessToken", out _).Should().BeTrue("response should contain accessToken");
        body.TryGetProperty("expiresAt", out _).Should().BeTrue("response should contain expiresAt");
        body.TryGetProperty("user", out _).Should().BeTrue("response should contain user");

        // JSON body should NOT contain refreshToken
        body.TryGetProperty("refreshToken", out _).Should().BeFalse("response body must NOT contain refreshToken");
    }

    // -----------------------------------------------------------------------
    // Test 2: Login with RememberMe sets persistent cookie (Max-Age)
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Login_WithRememberMe_SetsPersistentCookie()
    {
        // Arrange
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword, rememberMe = true };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var setCookieHeader = cookies!.FirstOrDefault(c => c.Contains("refresh_token"));
        setCookieHeader.Should().NotBeNull();

        // Should have max-age of approximately 30 days (2592000 seconds)
        setCookieHeader.Should().Contain("max-age=2592000", "remember me login should set 30-day Max-Age");
    }

    // -----------------------------------------------------------------------
    // Test 3: Login without RememberMe sets session cookie (no Max-Age)
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Login_WithoutRememberMe_SetsSessionCookie()
    {
        // Arrange
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword, rememberMe = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var setCookieHeader = cookies!.FirstOrDefault(c => c.Contains("refresh_token"));
        setCookieHeader.Should().NotBeNull();

        // Should NOT have max-age (session cookie -- expires when browser closes)
        setCookieHeader.Should().NotContain("max-age", "non-remember-me login should set session cookie without Max-Age");
    }

    // -----------------------------------------------------------------------
    // Test 4: Refresh with cookie returns new access token and sets new cookie
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Refresh_WithCookie_ReturnsNewAccessToken()
    {
        // Arrange -- login first to get a cookie
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Extract the refresh_token cookie value from Set-Cookie header
        loginResponse.Headers.TryGetValues("Set-Cookie", out var loginCookies).Should().BeTrue();
        var refreshCookieHeader = loginCookies!.First(c => c.Contains("refresh_token"));
        var cookieValue = ExtractCookieValue(refreshCookieHeader, "refresh_token");
        cookieValue.Should().NotBeNullOrEmpty();

        // Act -- call refresh endpoint with the cookie
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"refresh_token={cookieValue}");

        var refreshResponse = await _client.SendAsync(refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Should set a new refresh_token cookie
        refreshResponse.Headers.TryGetValues("Set-Cookie", out var refreshCookies).Should().BeTrue("refresh should set a new cookie");
        var newCookieHeader = refreshCookies!.FirstOrDefault(c => c.Contains("refresh_token"));
        newCookieHeader.Should().NotBeNull("refresh should set a new refresh_token cookie");

        // JSON body should contain accessToken, expiresAt, user
        var body = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("accessToken", out _).Should().BeTrue("response should contain accessToken");
        body.TryGetProperty("expiresAt", out _).Should().BeTrue("response should contain expiresAt");
        body.TryGetProperty("user", out _).Should().BeTrue("response should contain user");

        // JSON body should NOT contain refreshToken
        body.TryGetProperty("refreshToken", out _).Should().BeFalse("response body must NOT contain refreshToken");
    }

    // -----------------------------------------------------------------------
    // Test 5: Refresh without cookie returns 401
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        // Act -- call refresh endpoint without any cookie
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        var refreshResponse = await _client.SendAsync(refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -----------------------------------------------------------------------
    // Test 6: Logout clears the refresh token cookie
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Logout_ClearsRefreshTokenCookie()
    {
        // Arrange -- login first to get tokens
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrEmpty();

        // Extract refresh token cookie
        loginResponse.Headers.TryGetValues("Set-Cookie", out var loginCookies).Should().BeTrue();
        var refreshCookieHeader = loginCookies!.First(c => c.Contains("refresh_token"));
        var cookieValue = ExtractCookieValue(refreshCookieHeader, "refresh_token");

        // Act -- call logout with Bearer token and cookie
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
        logoutRequest.Headers.Add("Cookie", $"refresh_token={cookieValue}");

        var logoutResponse = await _client.SendAsync(logoutRequest);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Should have a Set-Cookie header that clears/expires the refresh_token
        logoutResponse.Headers.TryGetValues("Set-Cookie", out var logoutCookies).Should().BeTrue("logout should clear the cookie");
        var clearCookieHeader = logoutCookies!.FirstOrDefault(c => c.Contains("refresh_token"));
        clearCookieHeader.Should().NotBeNull("logout should clear the refresh_token cookie");

        // The clearing cookie should have an expired date or max-age=0
        var isCleared = clearCookieHeader!.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase)
            || clearCookieHeader.Contains("max-age=0", StringComparison.OrdinalIgnoreCase);
        isCleared.Should().BeTrue("logout cookie should be expired to clear it from the browser");
    }

    // -----------------------------------------------------------------------
    // Test 7: Refresh preserves RememberMe on token rotation
    // -----------------------------------------------------------------------
    [Fact]
    public async Task Refresh_PreservesRememberMe_OnRotation()
    {
        // Arrange -- login with rememberMe=true
        var loginPayload = new { email = CustomWebApplicationFactory.TestUserEmail, password = CustomWebApplicationFactory.TestUserPassword, rememberMe = true };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        loginResponse.Headers.TryGetValues("Set-Cookie", out var loginCookies).Should().BeTrue();
        var loginCookieHeader = loginCookies!.First(c => c.Contains("refresh_token"));
        loginCookieHeader.Should().Contain("max-age=2592000", "initial login should have 30-day max-age");

        var cookieValue = ExtractCookieValue(loginCookieHeader, "refresh_token");

        // Act -- refresh with the cookie
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"refresh_token={cookieValue}");

        var refreshResponse = await _client.SendAsync(refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        refreshResponse.Headers.TryGetValues("Set-Cookie", out var refreshCookies).Should().BeTrue();
        var refreshCookieHeader = refreshCookies!.First(c => c.Contains("refresh_token"));

        // The rotated cookie should ALSO have max-age (rememberMe preserved)
        refreshCookieHeader.Should().Contain("max-age=2592000", "refreshed cookie should preserve RememberMe max-age");
    }

    // -----------------------------------------------------------------------
    // Helper: Extract cookie value from Set-Cookie header
    // -----------------------------------------------------------------------
    private static string? ExtractCookieValue(string setCookieHeader, string cookieName)
    {
        // Format: "refresh_token=<value>; path=/api/auth; httponly; samesite=lax"
        var parts = setCookieHeader.Split(';');
        var nameValue = parts.FirstOrDefault(p => p.Trim().StartsWith(cookieName + "="));
        if (nameValue == null) return null;

        var eqIndex = nameValue.IndexOf('=');
        return eqIndex >= 0 ? nameValue[(eqIndex + 1)..].Trim() : null;
    }
}

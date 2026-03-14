using Clinical.Infrastructure.Hubs;
using Clinical.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Clinical.Unit.Tests.Features;

public class OsdiNotificationServiceTests
{
    private readonly IHubContext<OsdiHub> _hubContext = Substitute.For<IHubContext<OsdiHub>>();
    private readonly ILogger<OsdiNotificationService> _logger = Substitute.For<ILogger<OsdiNotificationService>>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();

    public OsdiNotificationServiceTests()
    {
        var hubClients = Substitute.For<IHubClients>();
        hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _hubContext.Clients.Returns(hubClients);
    }

    [Fact]
    public async Task NotifyOsdiSubmittedAsync_SendsToVisitGroup()
    {
        // Arrange
        var service = new OsdiNotificationService(_hubContext, _logger);
        var visitId = Guid.NewGuid();

        // Act
        await service.NotifyOsdiSubmittedAsync(visitId, 45.5m, "Severe", CancellationToken.None);

        // Assert
        _hubContext.Clients.Received(1).Group($"visit-{visitId}");
        await _clientProxy.Received(1).SendCoreAsync(
            "OsdiSubmitted",
            Arg.Is<object?[]>(args => args.Length == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyOsdiSubmittedAsync_SignalRFailure_LogsWarningDoesNotThrow()
    {
        // Arrange
        _clientProxy.SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("SignalR connection lost"));

        var service = new OsdiNotificationService(_hubContext, _logger);
        var visitId = Guid.NewGuid();

        // Act
        var act = async () => await service.NotifyOsdiSubmittedAsync(visitId, 45.5m, "Severe", CancellationToken.None);

        // Assert - should not throw
        await act.Should().NotThrowAsync();

        // Assert - LogWarning was called with the exception
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("OsdiSubmitted")),
            Arg.Is<Exception>(ex => ex.Message == "SignalR connection lost"),
            Arg.Any<Func<object, Exception?, string>>());
    }
}

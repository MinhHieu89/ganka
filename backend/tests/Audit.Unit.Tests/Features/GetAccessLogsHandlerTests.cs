using Audit.Application.Features;
using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Audit.Unit.Tests.Features;

public class GetAccessLogsHandlerTests
{
    private readonly IAuditReadRepository _repository = Substitute.For<IAuditReadRepository>();

    private GetAccessLogsHandler CreateSut() => new(_repository);

    private static AccessLog CreateTestAccessLog(
        AccessAction action = AccessAction.ApiRequest,
        string resource = "/api/patients",
        string ipAddress = "127.0.0.1",
        int statusCode = 200)
    {
        return AccessLog.Create(
            Guid.NewGuid(),
            "user@test.com",
            action,
            resource,
            ipAddress,
            "TestAgent",
            statusCode,
            Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ReturnsFilteredAccessLogs()
    {
        // Arrange
        var log1 = CreateTestAccessLog(AccessAction.Login, "/api/auth/login");
        var log2 = CreateTestAccessLog(AccessAction.ApiRequest, "/api/patients");
        var logs = new List<AccessLog> { log1, log2 };

        _repository.AccessLogs.Returns(logs.AsAsyncQueryable());

        var query = new GetAccessLogsQuery(PageSize: 50);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.Data.Should().HaveCount(2);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        _repository.AccessLogs.Returns(new List<AccessLog>().AsAsyncQueryable());

        var query = new GetAccessLogsQuery();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.Data.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ClampsPageSize()
    {
        // Arrange
        _repository.AccessLogs.Returns(new List<AccessLog>().AsAsyncQueryable());

        // PageSize 500 should be clamped to 200
        var query = new GetAccessLogsQuery(PageSize: 500);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.PageSize.Should().Be(200);
    }
}

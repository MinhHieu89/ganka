using Audit.Application.Features;
using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Audit.Unit.Tests.Features;

public class GetAuditLogsHandlerTests
{
    private readonly IAuditReadRepository _repository = Substitute.For<IAuditReadRepository>();

    private GetAuditLogsHandler CreateSut() => new(_repository);

    private static AuditLog CreateTestAuditLog(
        Guid? userId = null,
        string userEmail = "user@test.com",
        string entityName = "User",
        AuditAction action = AuditAction.Updated,
        DateTime? timestamp = null)
    {
        return AuditLog.Create(
            userId ?? Guid.NewGuid(),
            userEmail,
            entityName,
            Guid.NewGuid().ToString(),
            action,
            "[]",
            Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ReturnsFilteredLogs()
    {
        // Arrange
        var log1 = CreateTestAuditLog(entityName: "User");
        var log2 = CreateTestAuditLog(entityName: "Role");
        var logs = new List<AuditLog> { log1, log2 };

        _repository.AuditLogs.Returns(logs.AsAsyncQueryable());

        var query = new GetAuditLogsQuery(PageSize: 50);
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
        _repository.AuditLogs.Returns(new List<AuditLog>().AsAsyncQueryable());

        var query = new GetAuditLogsQuery();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.Data.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AppliesPagination_ClampsPageSize()
    {
        // Arrange
        var logs = Enumerable.Range(0, 5).Select(_ => CreateTestAuditLog()).ToList();
        _repository.AuditLogs.Returns(logs.AsAsyncQueryable());

        // PageSize 300 should be clamped to 200
        var query = new GetAuditLogsQuery(PageSize: 300);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.PageSize.Should().Be(200);
    }

    [Fact]
    public async Task Handle_AppliesPagination_MinPageSize()
    {
        // Arrange
        var logs = Enumerable.Range(0, 3).Select(_ => CreateTestAuditLog()).ToList();
        _repository.AuditLogs.Returns(logs.AsAsyncQueryable());

        // PageSize 0 should be clamped to 1
        var query = new GetAuditLogsQuery(PageSize: 0);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.PageSize.Should().Be(1);
        result.Data.Should().HaveCount(1);
    }
}

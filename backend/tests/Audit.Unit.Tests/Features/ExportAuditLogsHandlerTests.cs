using Audit.Application.Features;
using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Audit.Unit.Tests.Features;

public class ExportAuditLogsHandlerTests
{
    private readonly IAuditReadRepository _repository = Substitute.For<IAuditReadRepository>();

    private ExportAuditLogsHandler CreateSut() => new(_repository);

    private static AuditLog CreateTestAuditLog(
        string userEmail = "user@test.com",
        string entityName = "User",
        AuditAction action = AuditAction.Created)
    {
        return AuditLog.Create(
            Guid.NewGuid(),
            userEmail,
            entityName,
            Guid.NewGuid().ToString(),
            action,
            "[]",
            Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ReturnsExportData()
    {
        // Arrange
        var log1 = CreateTestAuditLog("admin@test.com", "User", AuditAction.Created);
        var log2 = CreateTestAuditLog("admin@test.com", "Role", AuditAction.Updated);
        var logs = new List<AuditLog> { log1, log2 };

        _repository.AuditLogs.Returns(logs.AsAsyncQueryable());

        var query = new ExportAuditLogsQuery();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.FileContents.Should().NotBeEmpty();
        result.FileName.Should().StartWith("audit-logs-");
        result.FileName.Should().EndWith(".csv");

        var csv = System.Text.Encoding.UTF8.GetString(result.FileContents);
        csv.Should().Contain("Id,Timestamp,UserEmail,EntityName,EntityId,Action,Changes");
        csv.Should().Contain("admin@test.com");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyExport()
    {
        // Arrange
        _repository.AuditLogs.Returns(new List<AuditLog>().AsAsyncQueryable());

        var query = new ExportAuditLogsQuery();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query);

        // Assert
        result.FileContents.Should().NotBeEmpty(); // Still has header row
        result.FileName.Should().StartWith("audit-logs-");

        var csv = System.Text.Encoding.UTF8.GetString(result.FileContents);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1); // Header only
    }
}

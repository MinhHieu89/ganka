using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Optical.Application.Features.Stocktaking;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// TDD tests for stocktaking handlers:
/// StartStocktakingSession, RecordStocktakingItem, CompleteStocktaking, GetDiscrepancyReport.
/// Follows the Wolverine static handler pattern established in Pharmacy.Unit.Tests.
/// </summary>
public class StocktakingHandlerTests
{
    private readonly IStocktakingRepository _stocktakingRepo = Substitute.For<IStocktakingRepository>();
    private readonly IFrameRepository _frameRepo = Substitute.For<IFrameRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultUserId = Guid.NewGuid();
    private static readonly Guid DefaultSessionId = Guid.NewGuid();
    private static readonly BranchId BranchId = new(DefaultBranchId);

    public StocktakingHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(DefaultUserId);
    }

    private static StocktakingSession CreateInProgressSession(string name = "Test Stocktake") =>
        StocktakingSession.Create(name, DefaultUserId, BranchId);

    #region StartStocktakingSession Tests

    [Fact]
    public async Task StartStocktakingSession_NoActiveSession_CreatesNewSession()
    {
        // Arrange
        _stocktakingRepo.GetCurrentSessionAsync(Arg.Any<CancellationToken>())
            .Returns((StocktakingSession?)null);

        var command = new StartStocktakingSessionCommand("Monthly Stocktake March 2026", null);

        // Act
        var result = await StartStocktakingSessionHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _stocktakingRepo.Received(1).Add(Arg.Any<StocktakingSession>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartStocktakingSession_ActiveSessionExists_ReturnsError()
    {
        // Arrange
        var existingSession = CreateInProgressSession("Existing Stocktake");
        _stocktakingRepo.GetCurrentSessionAsync(Arg.Any<CancellationToken>())
            .Returns(existingSession);

        var command = new StartStocktakingSessionCommand("New Stocktake", null);

        // Act
        var result = await StartStocktakingSessionHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already in progress");
        _stocktakingRepo.DidNotReceive().Add(Arg.Any<StocktakingSession>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartStocktakingSession_SetsStartedByIdFromCurrentUser()
    {
        // Arrange
        _stocktakingRepo.GetCurrentSessionAsync(Arg.Any<CancellationToken>())
            .Returns((StocktakingSession?)null);

        StocktakingSession? capturedSession = null;
        _stocktakingRepo.When(r => r.Add(Arg.Any<StocktakingSession>()))
            .Do(callInfo => capturedSession = callInfo.Arg<StocktakingSession>());

        var command = new StartStocktakingSessionCommand("March 2026", null);

        // Act
        await StartStocktakingSessionHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        capturedSession.Should().NotBeNull();
        capturedSession!.StartedById.Should().Be(DefaultUserId);
        capturedSession.Status.Should().Be(StocktakingStatus.InProgress);
    }

    #endregion

    #region RecordStocktakingItem Tests

    [Fact]
    public async Task RecordStocktakingItem_ValidBarcode_RecordsItemAndReturnsDto()
    {
        // Arrange
        var session = CreateInProgressSession();
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var frame = Frame.Create(
            brand: "Rayban",
            model: "RB5154",
            color: "Black",
            lensWidth: 52,
            bridgeWidth: 18,
            templeLength: 140,
            material: FrameMaterial.Plastic,
            type: FrameType.FullRim,
            gender: FrameGender.Unisex,
            sellingPrice: 2_000_000m,
            costPrice: 800_000m,
            barcode: "8930000000010",
            branchId: BranchId);

        _frameRepo.GetByBarcodeAsync("8930000000010", Arg.Any<CancellationToken>())
            .Returns(frame);

        var command = new RecordStocktakingItemCommand(
            SessionId: DefaultSessionId,
            Barcode: "8930000000010",
            PhysicalCount: 3);

        // Act
        var result = await RecordStocktakingItemHandler.Handle(
            command, _stocktakingRepo, _frameRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Barcode.Should().Be("8930000000010");
        dto.PhysicalCount.Should().Be(3);
        dto.FrameName.Should().NotBeNullOrEmpty();
        dto.FrameId.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordStocktakingItem_UnknownBarcode_RecordsWithNullFrame()
    {
        // Arrange
        var session = CreateInProgressSession();
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _frameRepo.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Frame?)null);

        var command = new RecordStocktakingItemCommand(
            SessionId: DefaultSessionId,
            Barcode: "9999999999991",
            PhysicalCount: 1);

        // Act
        var result = await RecordStocktakingItemHandler.Handle(
            command, _stocktakingRepo, _frameRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.FrameId.Should().BeNull();
        dto.FrameName.Should().BeNull();
        dto.SystemCount.Should().Be(0); // no frame => system count is 0
    }

    [Fact]
    public async Task RecordStocktakingItem_SessionNotFound_ReturnsError()
    {
        // Arrange
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((StocktakingSession?)null);

        var command = new RecordStocktakingItemCommand(
            SessionId: Guid.NewGuid(),
            Barcode: "8930000000010",
            PhysicalCount: 2);

        // Act
        var result = await RecordStocktakingItemHandler.Handle(
            command, _stocktakingRepo, _frameRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordStocktakingItem_DuplicateBarcode_Upserts()
    {
        // Arrange
        var session = CreateInProgressSession();
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _frameRepo.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Frame?)null);

        var command1 = new RecordStocktakingItemCommand(DefaultSessionId, "8930000000010", 2);
        var command2 = new RecordStocktakingItemCommand(DefaultSessionId, "8930000000010", 5);

        // Act - record same barcode twice
        await RecordStocktakingItemHandler.Handle(
            command1, _stocktakingRepo, _frameRepo, _unitOfWork, CancellationToken.None);
        var result = await RecordStocktakingItemHandler.Handle(
            command2, _stocktakingRepo, _frameRepo, _unitOfWork, CancellationToken.None);

        // Assert - upsert: only one item in session, physical count updated to 5
        result.IsSuccess.Should().BeTrue();
        result.Value.PhysicalCount.Should().Be(5);
        session.Items.Should().HaveCount(1); // upsert - not 2
    }

    #endregion

    #region CompleteStocktaking Tests

    [Fact]
    public async Task CompleteStocktaking_InProgressSession_CompletesAndSaves()
    {
        // Arrange
        var session = CreateInProgressSession();
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new CompleteStocktakingCommand(SessionId: DefaultSessionId, Notes: "All done");

        // Act
        var result = await CompleteStocktakingHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(StocktakingStatus.Completed);
        session.CompletedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteStocktaking_SessionNotFound_ReturnsError()
    {
        // Arrange
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((StocktakingSession?)null);

        var command = new CompleteStocktakingCommand(SessionId: Guid.NewGuid(), Notes: null);

        // Act
        var result = await CompleteStocktakingHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteStocktaking_AlreadyCompleted_ReturnsError()
    {
        // Arrange
        var session = CreateInProgressSession();
        session.Complete(); // complete once first
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new CompleteStocktakingCommand(SessionId: DefaultSessionId, Notes: null);

        // Act
        var result = await CompleteStocktakingHandler.Handle(
            command, _stocktakingRepo, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetDiscrepancyReport Tests

    [Fact]
    public async Task GetDiscrepancyReport_SessionWithItems_ReturnsSummary()
    {
        // Arrange
        var session = CreateInProgressSession("March 2026 Stocktake");
        // Record items: over count, under count, missing from system
        session.RecordItem("8930000000010", physicalCount: 5, systemCount: 3, frameId: Guid.NewGuid(), frameName: "Rayban RB5154"); // over
        session.RecordItem("8930000000027", physicalCount: 2, systemCount: 4, frameId: Guid.NewGuid(), frameName: "Oakley OX1"); // under
        session.RecordItem("9999999999991", physicalCount: 1, systemCount: 0, frameId: null, frameName: null); // missing from system
        session.Complete();

        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var query = new GetDiscrepancyReportQuery(SessionId: DefaultSessionId);

        // Act
        var result = await GetDiscrepancyReportHandler.Handle(query, _stocktakingRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.TotalScanned.Should().Be(3);
        report.TotalDiscrepancies.Should().Be(3); // all 3 items have discrepancy
        report.OverCount.Should().Be(1);
        report.UnderCount.Should().Be(1);
        report.MissingFromSystem.Should().Be(1);
        report.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDiscrepancyReport_SessionNotFound_ReturnsError()
    {
        // Arrange
        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((StocktakingSession?)null);

        var query = new GetDiscrepancyReportQuery(SessionId: Guid.NewGuid());

        // Act
        var result = await GetDiscrepancyReportHandler.Handle(query, _stocktakingRepo, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetDiscrepancyReport_MatchingCounts_ZeroDiscrepancies()
    {
        // Arrange
        var session = CreateInProgressSession("Clean Stocktake");
        session.RecordItem("8930000000010", physicalCount: 5, systemCount: 5, frameId: Guid.NewGuid(), frameName: "Frame A");
        session.RecordItem("8930000000027", physicalCount: 3, systemCount: 3, frameId: Guid.NewGuid(), frameName: "Frame B");
        session.Complete();

        _stocktakingRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var query = new GetDiscrepancyReportQuery(SessionId: DefaultSessionId);

        // Act
        var result = await GetDiscrepancyReportHandler.Handle(query, _stocktakingRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.TotalScanned.Should().Be(2);
        report.TotalDiscrepancies.Should().Be(0);
        report.OverCount.Should().Be(0);
        report.UnderCount.Should().Be(0);
        report.MissingFromSystem.Should().Be(0);
    }

    #endregion
}

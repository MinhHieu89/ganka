using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Optical.Application.Features.Warranty;
using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// TDD tests for warranty claim handler: GetWarrantyClaims.
/// Follows the Wolverine static handler pattern established in Pharmacy.Unit.Tests.
/// </summary>
public class WarrantyHandlerTests
{
    private readonly IWarrantyClaimRepository _warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultOrderId = Guid.NewGuid();

    // Helper to create a test WarrantyClaim entity via factory
    private static WarrantyClaim CreateTestClaim(WarrantyResolution resolution = WarrantyResolution.Repair) =>
        WarrantyClaim.Create(
            glassesOrderId: DefaultOrderId,
            claimDate: DateTime.UtcNow,
            resolution: resolution,
            assessmentNotes: "Frame hinge broken",
            discountAmount: resolution == WarrantyResolution.Discount ? 200_000m : null);

    #region GetWarrantyClaims Tests

    [Fact]
    public async Task GetWarrantyClaims_NoFilter_ReturnsPaginatedResults()
    {
        // Arrange
        var claims = new List<WarrantyClaim>
        {
            CreateTestClaim(WarrantyResolution.Repair),
            CreateTestClaim(WarrantyResolution.Replace)
        };
        const int totalCount = 2;

        _warrantyRepo.GetAllAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(claims);
        _warrantyRepo.GetTotalCountAsync(null, Arg.Any<CancellationToken>())
            .Returns(totalCount);

        var query = new GetWarrantyClaimsQuery(ApprovalStatusFilter: null, Page: 1, PageSize: 20);

        // Act
        var result = await GetWarrantyClaimsHandler.Handle(query, _warrantyRepo, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetWarrantyClaims_WithApprovalStatusFilter_DelegatesToRepository()
    {
        // Arrange
        var pendingClaims = new List<WarrantyClaim>
        {
            CreateTestClaim(WarrantyResolution.Replace)
        };
        const int pendingCount = 1;
        const int pendingStatusFilter = (int)WarrantyApprovalStatus.Pending;

        _warrantyRepo.GetAllAsync(pendingStatusFilter, 1, 20, Arg.Any<CancellationToken>())
            .Returns(pendingClaims);
        _warrantyRepo.GetTotalCountAsync(pendingStatusFilter, Arg.Any<CancellationToken>())
            .Returns(pendingCount);

        var query = new GetWarrantyClaimsQuery(ApprovalStatusFilter: pendingStatusFilter, Page: 1, PageSize: 20);

        // Act
        var result = await GetWarrantyClaimsHandler.Handle(query, _warrantyRepo, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(pendingCount);
        await _warrantyRepo.Received(1).GetAllAsync(pendingStatusFilter, 1, 20, Arg.Any<CancellationToken>());
        await _warrantyRepo.Received(1).GetTotalCountAsync(pendingStatusFilter, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetWarrantyClaims_EmptyList_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        _warrantyRepo.GetAllAsync(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<WarrantyClaim>());
        _warrantyRepo.GetTotalCountAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetWarrantyClaimsQuery(ApprovalStatusFilter: null, Page: 1, PageSize: 20);

        // Act
        var result = await GetWarrantyClaimsHandler.Handle(query, _warrantyRepo, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetWarrantyClaims_MapsToDtoCorrectly()
    {
        // Arrange
        var claim = CreateTestClaim(WarrantyResolution.Repair);
        _warrantyRepo.GetAllAsync(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<WarrantyClaim> { claim });
        _warrantyRepo.GetTotalCountAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetWarrantyClaimsQuery(ApprovalStatusFilter: null, Page: 1, PageSize: 20);

        // Act
        var result = await GetWarrantyClaimsHandler.Handle(query, _warrantyRepo, CancellationToken.None);

        // Assert
        var dto = result.Items.First();
        dto.GlassesOrderId.Should().Be(DefaultOrderId);
        dto.Resolution.Should().Be((int)WarrantyResolution.Repair);
        dto.ApprovalStatus.Should().Be((int)WarrantyApprovalStatus.Pending);
        dto.RequiresApproval.Should().BeFalse(); // Repair does not require approval
    }

    [Fact]
    public async Task GetWarrantyClaims_ReplaceResolution_RequiresApprovalIsTrue()
    {
        // Arrange
        var replaceClaim = CreateTestClaim(WarrantyResolution.Replace);
        _warrantyRepo.GetAllAsync(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<WarrantyClaim> { replaceClaim });
        _warrantyRepo.GetTotalCountAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetWarrantyClaimsQuery(ApprovalStatusFilter: null, Page: 1, PageSize: 20);

        // Act
        var result = await GetWarrantyClaimsHandler.Handle(query, _warrantyRepo, CancellationToken.None);

        // Assert
        result.Items.First().RequiresApproval.Should().BeTrue();
    }

    #endregion

    #region CreateWarrantyClaim Tests

    [Fact]
    public async Task CreateWarrantyClaim_ValidRepairClaim_AutoApproves()
    {
        // Arrange
        var order = GlassesOrder.Create(
            Guid.NewGuid(), "Nguyen Van A", Guid.NewGuid(), Guid.NewGuid(),
            ProcessingType.InHouse, null, 5_000_000m, null, null,
            new BranchId(DefaultBranchId));
        // Deliver the order to set warranty
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: order.Id,
            Resolution: (int)WarrantyResolution.Repair,
            AssessmentNotes: "Frame hinge broken",
            DiscountAmount: null);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        warrantyRepo.Received(1).Add(Arg.Is<WarrantyClaim>(c =>
            c.Resolution == WarrantyResolution.Repair &&
            c.ApprovalStatus == WarrantyApprovalStatus.Approved));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWarrantyClaim_ValidReplaceClaim_SetsPending()
    {
        // Arrange
        var order = GlassesOrder.Create(
            Guid.NewGuid(), "Nguyen Van B", Guid.NewGuid(), Guid.NewGuid(),
            ProcessingType.InHouse, null, 5_000_000m, null, null,
            new BranchId(DefaultBranchId));
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: order.Id,
            Resolution: (int)WarrantyResolution.Replace,
            AssessmentNotes: "Frame needs full replacement",
            DiscountAmount: null);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        warrantyRepo.Received(1).Add(Arg.Is<WarrantyClaim>(c =>
            c.Resolution == WarrantyResolution.Replace &&
            c.ApprovalStatus == WarrantyApprovalStatus.Pending));
    }

    [Fact]
    public async Task CreateWarrantyClaim_ValidDiscountClaim_AutoApproves()
    {
        // Arrange
        var order = GlassesOrder.Create(
            Guid.NewGuid(), "Nguyen Van C", Guid.NewGuid(), Guid.NewGuid(),
            ProcessingType.InHouse, null, 5_000_000m, null, null,
            new BranchId(DefaultBranchId));
        order.TransitionTo(GlassesOrderStatus.Processing);
        order.TransitionTo(GlassesOrderStatus.Received);
        order.TransitionTo(GlassesOrderStatus.Ready);
        order.TransitionTo(GlassesOrderStatus.Delivered);

        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: order.Id,
            Resolution: (int)WarrantyResolution.Discount,
            AssessmentNotes: "Minor defect, applying discount",
            DiscountAmount: 200_000m);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        warrantyRepo.Received(1).Add(Arg.Is<WarrantyClaim>(c =>
            c.Resolution == WarrantyResolution.Discount &&
            c.ApprovalStatus == WarrantyApprovalStatus.Approved));
    }

    [Fact]
    public async Task CreateWarrantyClaim_OrderNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GlassesOrder?)null);
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: Guid.NewGuid(),
            Resolution: (int)WarrantyResolution.Repair,
            AssessmentNotes: "Some notes",
            DiscountAmount: null);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        warrantyRepo.DidNotReceive().Add(Arg.Any<WarrantyClaim>());
    }

    [Fact]
    public async Task CreateWarrantyClaim_OrderNotUnderWarranty_ReturnsValidationError()
    {
        // Arrange - order not delivered (no DeliveredAt), so not under warranty
        var order = GlassesOrder.Create(
            Guid.NewGuid(), "Nguyen Van D", Guid.NewGuid(), Guid.NewGuid(),
            ProcessingType.InHouse, null, 5_000_000m, null, null,
            new BranchId(DefaultBranchId));
        // Only move to Processing, not delivered yet
        order.TransitionTo(GlassesOrderStatus.Processing);

        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: order.Id,
            Resolution: (int)WarrantyResolution.Repair,
            AssessmentNotes: "Frame issue",
            DiscountAmount: null);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        warrantyRepo.DidNotReceive().Add(Arg.Any<WarrantyClaim>());
    }

    [Fact]
    public async Task CreateWarrantyClaim_EmptyNotes_ReturnsValidationError()
    {
        // Arrange
        var orderRepo = Substitute.For<IGlassesOrderRepository>();
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var validator = new CreateWarrantyClaimCommandValidator();

        var command = new CreateWarrantyClaimCommand(
            GlassesOrderId: Guid.NewGuid(),
            Resolution: (int)WarrantyResolution.Repair,
            AssessmentNotes: "",
            DiscountAmount: null);

        // Act
        var result = await CreateWarrantyClaimHandler.Handle(
            command, orderRepo, warrantyRepo, unitOfWork, validator, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        orderRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ApproveWarrantyClaim Tests

    [Fact]
    public async Task ApproveWarrantyClaim_PendingReplaceClaim_Approve_TransitionsToApproved()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            DefaultOrderId, DateTime.UtcNow, WarrantyResolution.Replace,
            "Frame needs replacement", null);
        // claim is in Pending state

        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var managerId = Guid.NewGuid();
        currentUser.UserId.Returns(managerId);
        warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        var command = new ApproveWarrantyClaimCommand(
            ClaimId: claim.Id,
            IsApproved: true,
            Notes: "Approved for replacement");

        // Act
        var result = await ApproveWarrantyClaimHandler.Handle(
            command, warrantyRepo, unitOfWork, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        claim.ApprovalStatus.Should().Be(WarrantyApprovalStatus.Approved);
        claim.ApprovedById.Should().Be(managerId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveWarrantyClaim_PendingReplaceClaim_Reject_TransitionsToRejected()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            DefaultOrderId, DateTime.UtcNow, WarrantyResolution.Replace,
            "Frame needs replacement", null);

        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var managerId = Guid.NewGuid();
        currentUser.UserId.Returns(managerId);
        warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        var command = new ApproveWarrantyClaimCommand(
            ClaimId: claim.Id,
            IsApproved: false,
            Notes: "Damage not covered by warranty");

        // Act
        var result = await ApproveWarrantyClaimHandler.Handle(
            command, warrantyRepo, unitOfWork, currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        claim.ApprovalStatus.Should().Be(WarrantyApprovalStatus.Rejected);
        claim.ApprovedById.Should().Be(managerId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveWarrantyClaim_ClaimNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        warrantyRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((WarrantyClaim?)null);

        var command = new ApproveWarrantyClaimCommand(
            ClaimId: Guid.NewGuid(),
            IsApproved: true,
            Notes: null);

        // Act
        var result = await ApproveWarrantyClaimHandler.Handle(
            command, warrantyRepo, unitOfWork, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveWarrantyClaim_NonReplaceClaim_ReturnsValidationError()
    {
        // Arrange - Repair claim should not be approvable
        var claim = WarrantyClaim.Create(
            DefaultOrderId, DateTime.UtcNow, WarrantyResolution.Repair,
            "Frame hinge broken", null);
        // Auto-approve the Repair claim (simulate what CreateWarrantyClaim does)
        // We need to test that the handler rejects non-Replace claims

        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        var command = new ApproveWarrantyClaimCommand(
            ClaimId: claim.Id,
            IsApproved: true,
            Notes: null);

        // Act
        var result = await ApproveWarrantyClaimHandler.Handle(
            command, warrantyRepo, unitOfWork, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveWarrantyClaim_Reject_WithoutReason_ReturnsValidationError()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            DefaultOrderId, DateTime.UtcNow, WarrantyResolution.Replace,
            "Frame needs replacement", null);

        var warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        var command = new ApproveWarrantyClaimCommand(
            ClaimId: claim.Id,
            IsApproved: false,
            Notes: null); // No rejection reason

        // Act
        var result = await ApproveWarrantyClaimHandler.Handle(
            command, warrantyRepo, unitOfWork, currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}

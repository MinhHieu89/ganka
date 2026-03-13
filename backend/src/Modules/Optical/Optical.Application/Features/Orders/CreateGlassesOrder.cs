using FluentValidation;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Line item within a glasses order -- frame or lens selection.
/// </summary>
public sealed record GlassesOrderItemRequest(
    Guid? FrameId,
    Guid? LensCatalogItemId,
    string Description,
    decimal UnitPrice,
    int Quantity,
    string? DescriptionVi = null);

/// <summary>
/// Command to create a new glasses order linked to a patient visit and optical prescription.
/// </summary>
public sealed record CreateGlassesOrderCommand(
    Guid PatientId,
    string PatientName,
    Guid VisitId,
    Guid OpticalPrescriptionId,
    int ProcessingType,
    DateTime? EstimatedDeliveryDate,
    decimal TotalPrice,
    Guid? ComboPackageId,
    string? Notes,
    List<GlassesOrderItemRequest> Items);

/// <summary>
/// FluentValidation validator for <see cref="CreateGlassesOrderCommand"/>.
/// Enforces required fields and price constraints.
/// </summary>
public class CreateGlassesOrderValidator : AbstractValidator<CreateGlassesOrderCommand>
{
    public CreateGlassesOrderValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters.");
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.OpticalPrescriptionId).NotEmpty().WithMessage("Optical prescription ID is required.");
        RuleFor(x => x.TotalPrice).GreaterThan(0).WithMessage("Total price must be greater than zero.");
    }
}

/// <summary>
/// Wolverine static handler for creating a new glasses order.
/// Creates a <see cref="GlassesOrder"/> aggregate with <see cref="GlassesOrderStatus.Ordered"/> status,
/// adds line items, then persists via repository and unit of work.
/// </summary>
public static class CreateGlassesOrderHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateGlassesOrderCommand command,
        IGlassesOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Inline validation without FluentValidation DI to keep handler testable
        if (command.PatientId == Guid.Empty)
            return Result.Failure<Guid>(Error.Validation("Patient ID is required."));
        if (string.IsNullOrWhiteSpace(command.PatientName))
            return Result.Failure<Guid>(Error.Validation("Patient name is required."));
        if (command.VisitId == Guid.Empty)
            return Result.Failure<Guid>(Error.Validation("Visit ID is required."));
        if (command.OpticalPrescriptionId == Guid.Empty)
            return Result.Failure<Guid>(Error.Validation("Optical prescription ID is required."));
        if (command.TotalPrice <= 0)
            return Result.Failure<Guid>(Error.Validation("Total price must be greater than zero."));

        var branchId = new BranchId(currentUser.BranchId);

        var order = GlassesOrder.Create(
            patientId: command.PatientId,
            patientName: command.PatientName,
            visitId: command.VisitId,
            opticalPrescriptionId: command.OpticalPrescriptionId,
            processingType: (ProcessingType)command.ProcessingType,
            estimatedDeliveryDate: command.EstimatedDeliveryDate,
            totalPrice: command.TotalPrice,
            comboPackageId: command.ComboPackageId,
            notes: command.Notes,
            branchId: branchId);

        foreach (var item in command.Items)
        {
            var orderItem = GlassesOrderItem.Create(
                glassesOrderId: order.Id,
                frameId: item.FrameId,
                lensCatalogItemId: item.LensCatalogItemId,
                itemDescription: item.Description,
                itemDescriptionVi: item.DescriptionVi ?? string.Empty,
                unitPrice: item.UnitPrice,
                quantity: item.Quantity);
            order.AddItem(orderItem);
        }

        order.RaiseCreatedEvent();

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(order.Id);
    }
}

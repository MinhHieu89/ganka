using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to create a new draft invoice.
/// </summary>
public sealed record CreateInvoiceCommand(
    Guid PatientId,
    string PatientName,
    Guid? VisitId);

/// <summary>
/// Validator for <see cref="CreateInvoiceCommand"/>.
/// </summary>
public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters.");
    }
}

/// <summary>
/// Wolverine static handler for creating a new draft invoice.
/// Gets next invoice number via repository, creates Invoice via factory, persists via repository.
/// </summary>
public static class CreateInvoiceHandler
{
    public static async Task<Result<InvoiceDto>> Handle(
        CreateInvoiceCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateInvoiceCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<InvoiceDto>(Error.ValidationWithDetails(errors));
        }

        var invoiceNumber = await invoiceRepository.GetNextInvoiceNumberAsync(
            DateTime.UtcNow.Year, ct);

        var invoice = Invoice.Create(
            invoiceNumber,
            command.PatientId,
            command.PatientName,
            command.VisitId,
            new BranchId(currentUser.BranchId));

        invoiceRepository.Add(invoice);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToDto(invoice);
    }

    internal static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            VisitId: invoice.VisitId,
            PatientId: invoice.PatientId,
            PatientName: invoice.PatientName,
            Status: (int)invoice.Status,
            SubTotal: invoice.SubTotal,
            DiscountTotal: invoice.DiscountTotal,
            TotalAmount: invoice.TotalAmount,
            PaidAmount: invoice.PaidAmount,
            BalanceDue: invoice.BalanceDue,
            CashierShiftId: invoice.CashierShiftId,
            FinalizedById: invoice.FinalizedById,
            FinalizedAt: invoice.FinalizedAt,
            CreatedAt: invoice.CreatedAt,
            LineItems: invoice.LineItems.Select(li => new InvoiceLineItemDto(
                Id: li.Id,
                Description: li.Description,
                DescriptionVi: li.DescriptionVi,
                UnitPrice: li.UnitPrice,
                Quantity: li.Quantity,
                LineTotal: li.LineTotal,
                Department: (int)li.Department,
                SourceId: li.SourceId,
                SourceType: li.SourceType)).ToList(),
            Payments: invoice.Payments.Select(p => new PaymentDto(
                Id: p.Id,
                InvoiceId: p.InvoiceId,
                Method: (int)p.Method,
                Amount: p.Amount,
                Status: (int)p.Status,
                ReferenceNumber: p.ReferenceNumber,
                CardLast4: p.CardLast4,
                CardType: p.CardType,
                Notes: p.Notes,
                RecordedById: p.RecordedById,
                RecordedAt: p.RecordedAt,
                CashierShiftId: p.CashierShiftId,
                TreatmentPackageId: p.TreatmentPackageId,
                IsSplitPayment: p.IsSplitPayment,
                SplitSequence: p.SplitSequence)).ToList(),
            Discounts: invoice.Discounts.Select(d => new DiscountDto(
                Id: d.Id,
                InvoiceLineItemId: d.InvoiceLineItemId,
                Type: (int)d.Type,
                Value: d.Value,
                CalculatedAmount: d.CalculatedAmount,
                Reason: d.Reason,
                ApprovalStatus: (int)d.ApprovalStatus,
                RequestedById: d.RequestedById,
                RequestedAt: d.RequestedAt,
                ApprovedById: d.ApprovedById,
                ApprovedAt: d.ApprovedAt)).ToList(),
            Refunds: invoice.Refunds.Select(r => new RefundDto(
                Id: r.Id,
                InvoiceLineItemId: r.InvoiceLineItemId,
                Amount: r.Amount,
                Reason: r.Reason,
                Status: (int)r.Status,
                RequestedById: r.RequestedById,
                RequestedAt: r.RequestedAt,
                ApprovedById: r.ApprovedById,
                ApprovedAt: r.ApprovedAt,
                ProcessedById: r.ProcessedById,
                ProcessedAt: r.ProcessedAt)).ToList());
    }
}

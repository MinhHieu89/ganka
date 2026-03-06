using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
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
/// </summary>
public static class CreateInvoiceHandler
{
    public static Task<Result<InvoiceDto>> Handle(
        CreateInvoiceCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateInvoiceCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        throw new NotImplementedException("RED phase stub -- implement in Task 2");
    }
}

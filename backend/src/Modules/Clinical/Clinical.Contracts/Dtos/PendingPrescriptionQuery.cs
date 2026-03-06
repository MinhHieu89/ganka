namespace Clinical.Contracts.Dtos;

/// <summary>
/// Cross-module query to retrieve pending (not yet dispensed) drug prescriptions from the Clinical module.
/// Issued by Pharmacy.Presentation via IMessageBus to get the dispensing queue.
/// The handler lives in Clinical.Application and has access to DrugPrescription data.
/// </summary>
/// <param name="PatientId">Optional patient filter. When null, returns all pending prescriptions for the branch.</param>
public sealed record GetPendingPrescriptionsQuery(Guid? PatientId = null);

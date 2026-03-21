using FluentAssertions;
using NSubstitute;
using Patient.Application.Features;
using Patient.Application.Interfaces;
using Patient.Domain.Enums;

namespace Patient.Unit.Tests.Features;

/// <summary>
/// Tests for GetPatientListHandler verifying that IsActive filter is passed through
/// correctly to the repository for conditional patient visibility.
/// </summary>
public class GetPatientListHandlerTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();

    private Domain.Entities.Patient CreateTestPatient(string fullName, string phone, bool isActive = true)
    {
        var patient = Domain.Entities.Patient.Create(
            fullName, phone, PatientType.Medical,
            new Shared.Domain.BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001")),
            new DateTime(1990, 1, 1), Gender.Male);
        patient.SetSequence(2026, 1);
        if (!isActive)
            patient.Deactivate();
        return patient;
    }

    [Fact]
    public async Task Handle_IsActiveNull_PassesNullToRepository()
    {
        // Arrange - IsActive=null should show all patients (no filter)
        var query = new GetPatientListQuery(IsActive: null);
        _patientRepository.GetPagedAsync(
            1, 20, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Domain.Entities.Patient>(), 0));

        // Act
        var result = await GetPatientListHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _patientRepository.Received(1).GetPagedAsync(
            1, 20, null, null, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IsActiveTrue_PassesTrueToRepository()
    {
        // Arrange - IsActive=true should filter to active patients only
        var query = new GetPatientListQuery(IsActive: true);
        _patientRepository.GetPagedAsync(
            1, 20, null, null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns((new List<Domain.Entities.Patient>(), 0));

        // Act
        var result = await GetPatientListHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _patientRepository.Received(1).GetPagedAsync(
            1, 20, null, null, null, null, null, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IsActiveFalse_PassesFalseToRepository()
    {
        // Arrange - IsActive=false should filter to inactive patients only
        var query = new GetPatientListQuery(IsActive: false);
        _patientRepository.GetPagedAsync(
            1, 20, null, null, null, null, null, false, Arg.Any<CancellationToken>())
            .Returns((new List<Domain.Entities.Patient>(), 0));

        // Act
        var result = await GetPatientListHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _patientRepository.Received(1).GetPagedAsync(
            1, 20, null, null, null, null, null, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DefaultQuery_UsesNullIsActive()
    {
        // Arrange - default query should have IsActive=null (show all)
        var query = new GetPatientListQuery();
        _patientRepository.GetPagedAsync(
            1, 20, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Domain.Entities.Patient>(), 0));

        // Act
        var result = await GetPatientListHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

using FluentAssertions;
using NSubstitute;
using Patient.Application.Features;
using Patient.Application.Interfaces;
using Patient.Domain.Enums;

namespace Patient.Unit.Tests.Features;

/// <summary>
/// Tests for SearchPatientsHandler verifying that search terms are passed through
/// to the repository correctly for substring matching behavior.
/// </summary>
public class SearchPatientsHandlerTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();

    private Domain.Entities.Patient CreateTestPatient(string fullName, string phone, string patientCode)
    {
        var patient = Domain.Entities.Patient.Create(
            fullName, phone, PatientType.Medical,
            new Shared.Domain.BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001")),
            new DateTime(1990, 1, 1), Gender.Male);
        // Set patient code via reflection since SetSequence is the normal path
        patient.SetSequence(2026, 1);
        return patient;
    }

    [Fact]
    public async Task Handle_SubstringPatientCode_PassesTermToRepository()
    {
        // Arrange - partial patient code "0001" should match "GK-2026-0001" via Contains
        var term = "0001";
        var patient = CreateTestPatient("Nguyen Van A", "0901234567", "GK-2026-0001");
        _patientRepository.SearchAsync(term, 20, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Patient> { patient });

        var query = new SearchPatientsQuery(term);

        // Act
        var result = await SearchPatientsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        await _patientRepository.Received(1).SearchAsync(term, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SubstringPhone_PassesTermToRepository()
    {
        // Arrange - partial phone "6543" should match "0987654321" via Contains
        var term = "6543";
        var patient = CreateTestPatient("Tran Thi B", "0987654321", "GK-2026-0002");
        _patientRepository.SearchAsync(term, 20, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Patient> { patient });

        var query = new SearchPatientsQuery(term);

        // Act
        var result = await SearchPatientsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        await _patientRepository.Received(1).SearchAsync(term, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FullPatientCode_PassesTermToRepository()
    {
        // Arrange - full patient code should still work
        var term = "GK-2026-0001";
        var patient = CreateTestPatient("Nguyen Van A", "0901234567", "GK-2026-0001");
        _patientRepository.SearchAsync(term, 20, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Patient> { patient });

        var query = new SearchPatientsQuery(term);

        // Act
        var result = await SearchPatientsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_PhonePrefix_PassesTermToRepository()
    {
        // Arrange - phone prefix "098" should still work via Contains
        var term = "098";
        var patient = CreateTestPatient("Tran Thi B", "0987654321", "GK-2026-0002");
        _patientRepository.SearchAsync(term, 20, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Patient> { patient });

        var query = new SearchPatientsQuery(term);

        // Act
        var result = await SearchPatientsHandler.Handle(query, _patientRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}

using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Patient.Application.Features;
using Patient.Application.Interfaces;
using Scheduling.Contracts.Queries;
using Clinical.Contracts.Dtos;
using Treatment.Contracts.Queries;
using Wolverine;

namespace Patient.Unit.Tests.Features;

/// <summary>
/// Tests for GetDashboardStatsHandler verifying that dashboard statistics
/// are correctly returned from the patient repository and cross-module queries.
/// </summary>
public class GetDashboardStatsHandlerTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_ReturnsActivePatientCount()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(42);
        SetupDefaultBusMocks();

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPatients.Should().Be(42);
        await _patientRepository.Received(1).GetActiveCountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsZeroWhenNoPatientsExist()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        SetupDefaultBusMocks();

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPatients.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsTodayAppointmentCount_FromSchedulingModule()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(10);
        _bus.InvokeAsync<int>(Arg.Any<GetTodayAppointmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(5);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveVisitCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveTreatmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TodayAppointments.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ReturnsActiveVisitCount_FromClinicalModule()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(10);
        _bus.InvokeAsync<int>(Arg.Any<GetTodayAppointmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveVisitCountQuery>(), Arg.Any<CancellationToken>()).Returns(3);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveTreatmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveVisits.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ReturnsActiveTreatmentCount_FromTreatmentModule()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(10);
        _bus.InvokeAsync<int>(Arg.Any<GetTodayAppointmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveVisitCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveTreatmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(2);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveTreatments.Should().Be(2);
    }

    [Fact]
    public async Task Handle_GracefullyHandlesCrossModuleFailure()
    {
        // Arrange
        _patientRepository.GetActiveCountAsync(Arg.Any<CancellationToken>()).Returns(10);
        _bus.InvokeAsync<int>(Arg.Any<GetTodayAppointmentCountQuery>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Scheduling module unavailable"));
        _bus.InvokeAsync<int>(Arg.Any<GetActiveVisitCountQuery>(), Arg.Any<CancellationToken>()).Returns(3);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveTreatmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(2);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await GetDashboardStatsHandler.Handle(query, _patientRepository, _bus, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPatients.Should().Be(10);
        result.Value.TodayAppointments.Should().Be(0); // graceful degradation
        result.Value.ActiveVisits.Should().Be(3);
        result.Value.ActiveTreatments.Should().Be(2);
    }

    private void SetupDefaultBusMocks()
    {
        _bus.InvokeAsync<int>(Arg.Any<GetTodayAppointmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveVisitCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
        _bus.InvokeAsync<int>(Arg.Any<GetActiveTreatmentCountQuery>(), Arg.Any<CancellationToken>()).Returns(0);
    }
}

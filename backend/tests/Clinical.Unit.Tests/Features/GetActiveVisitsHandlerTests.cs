using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetActiveVisitsHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_WithActiveVisits_ReturnsMappedDtos()
    {
        // Arrange
        var visit1 = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, true);
        var visit2 = Visit.Create(
            Guid.NewGuid(), "Patient B", Guid.NewGuid(), "Dr. B",
            DefaultBranchId, false);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit1, visit2 });

        var query = new GetActiveVisitsQuery();

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].PatientName.Should().Be("Patient A");
        result[0].HasAllergies.Should().BeTrue();
        result[0].CurrentStage.Should().Be((int)WorkflowStage.Reception);
        result[1].PatientName.Should().Be("Patient B");
        result[1].HasAllergies.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoActiveVisits_ReturnsEmptyList()
    {
        // Arrange
        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit>());

        var query = new GetActiveVisitsQuery();

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            query, _visitRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_VisitWithAllergies_HasAllergiesFlagTrue()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient Allergy", Guid.NewGuid(), "Dr. C",
            DefaultBranchId, true);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].HasAllergies.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ActiveVisits_WaitMinutesCalculated()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient Wait", Guid.NewGuid(), "Dr. D",
            DefaultBranchId, false);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].WaitMinutes.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Handle_ActiveVisits_IncludesPatientAndDoctorInfo()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit = Visit.Create(
            patientId, "Tran Thi B", Guid.NewGuid(), "Dr. Nguyen",
            DefaultBranchId, false);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result[0].PatientId.Should().Be(patientId);
        result[0].PatientName.Should().Be("Tran Thi B");
        result[0].DoctorName.Should().Be("Dr. Nguyen");
    }

    [Fact]
    public async Task Handle_DraftVisit_IsCompletedFalse()
    {
        // Arrange -- Draft visit at Reception
        var visit = Visit.Create(
            Guid.NewGuid(), "Active Patient", Guid.NewGuid(), "Dr. E",
            DefaultBranchId, false);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result[0].IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_VisitAtPharmacyOptical_IsCompletedTrue()
    {
        // Arrange -- visit advanced to PharmacyOptical (done-today)
        var visit = Visit.Create(
            Guid.NewGuid(), "Completed Patient", Guid.NewGuid(), "Dr. F",
            DefaultBranchId, false);
        visit.AdvanceStage(WorkflowStage.RefractionVA);
        visit.AdvanceStage(WorkflowStage.DoctorExam);
        visit.AdvanceStage(WorkflowStage.Diagnostics);
        visit.AdvanceStage(WorkflowStage.DoctorReads);
        visit.AdvanceStage(WorkflowStage.Rx);
        visit.AdvanceStage(WorkflowStage.Cashier);
        visit.AdvanceStage(WorkflowStage.PharmacyOptical);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { visit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result[0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MixedVisits_CorrectIsCompletedFlag()
    {
        // Arrange
        var activeVisit = Visit.Create(
            Guid.NewGuid(), "Active", Guid.NewGuid(), "Dr. G",
            DefaultBranchId, false);

        var doneVisit = Visit.Create(
            Guid.NewGuid(), "Done", Guid.NewGuid(), "Dr. H",
            DefaultBranchId, false);
        doneVisit.AdvanceStage(WorkflowStage.RefractionVA);
        doneVisit.AdvanceStage(WorkflowStage.DoctorExam);
        doneVisit.AdvanceStage(WorkflowStage.Diagnostics);
        doneVisit.AdvanceStage(WorkflowStage.DoctorReads);
        doneVisit.AdvanceStage(WorkflowStage.Rx);
        doneVisit.AdvanceStage(WorkflowStage.Cashier);
        doneVisit.AdvanceStage(WorkflowStage.PharmacyOptical);

        _visitRepository.GetActiveVisitsIncludingDoneTodayAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Visit> { activeVisit, doneVisit });

        // Act
        var result = await GetActiveVisitsHandler.Handle(
            new GetActiveVisitsQuery(), _visitRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsCompleted.Should().BeFalse();
        result[1].IsCompleted.Should().BeTrue();
    }
}

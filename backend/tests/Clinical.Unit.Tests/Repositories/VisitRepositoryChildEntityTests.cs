using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Clinical.Infrastructure;
using Clinical.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Clinical.Unit.Tests.Repositories;

public class VisitRepositoryChildEntityTests : IDisposable
{
    private readonly ClinicalDbContext _dbContext;
    private readonly VisitRepository _repository;

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public VisitRepositoryChildEntityTests()
    {
        var options = new DbContextOptionsBuilder<ClinicalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ClinicalDbContext(options);
        _repository = new VisitRepository(_dbContext);
    }

    [Fact]
    public void AddRefraction_TracksEntityAsAdded()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        _dbContext.Visits.Add(visit);
        _dbContext.SaveChanges();

        var refraction = Refraction.Create(visit.Id, RefractionType.Manifest);

        // Act
        _repository.AddRefraction(refraction);

        // Assert
        var entry = _dbContext.Entry(refraction);
        entry.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public void AddDiagnosis_TracksEntityAsAdded()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        _dbContext.Visits.Add(visit);
        _dbContext.SaveChanges();

        var diagnosis = VisitDiagnosis.Create(
            visit.Id, "H04.121", "Dry Eye, right", "Kho mat, phai",
            Laterality.OD, DiagnosisRole.Primary, 1);

        // Act
        _repository.AddDiagnosis(diagnosis);

        // Assert
        var entry = _dbContext.Entry(diagnosis);
        entry.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public void AddAmendment_TracksEntityAsAdded()
    {
        // Arrange
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
        _dbContext.Visits.Add(visit);
        _dbContext.SaveChanges();
        // Sign the visit so we can create an amendment
        visit.SignOff(Guid.NewGuid());
        _dbContext.SaveChanges();

        var amendment = VisitAmendment.Create(
            visit.Id,
            Guid.NewGuid(),
            "doctor@test.com",
            "Corrected diagnosis",
            "[{\"FieldName\":\"Laterality\",\"OldValue\":\"OD\",\"NewValue\":\"OS\"}]");

        // Act
        _repository.AddAmendment(amendment);

        // Assert
        var entry = _dbContext.Entry(amendment);
        entry.State.Should().Be(EntityState.Added);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

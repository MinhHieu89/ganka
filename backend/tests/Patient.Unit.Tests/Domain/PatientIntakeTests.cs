using FluentAssertions;
using Patient.Domain.Entities;
using Patient.Domain.Enums;
using Shared.Domain;

namespace Patient.Unit.Tests;

/// <summary>
/// Tests for receptionist intake form fields on Patient entity:
/// Email, Occupation, OcularHistory, SystemicHistory, CurrentMedications,
/// ScreenTimeHours, WorkEnvironment, ContactLensUsage, LifestyleNotes.
/// </summary>
public class PatientIntakeTests
{
    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Domain.Entities.Patient CreateMedicalPatient()
    {
        return Domain.Entities.Patient.Create(
            "Nguyen Van B", "0901234567",
            PatientType.Medical, DefaultBranchId,
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Male);
    }

    [Fact]
    public void UpdateIntake_SetsAllNewFields()
    {
        var patient = CreateMedicalPatient();

        patient.UpdateIntake(
            fullName: "Nguyen Van B",
            phone: "0901234567",
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Male,
            address: "123 Hang Bong",
            cccd: "012345678901",
            email: "nguyenvanb@gmail.com",
            occupation: "Software Engineer",
            ocularHistory: "Near-sightedness since age 12",
            systemicHistory: "None",
            currentMedications: "Vitamin A",
            screenTimeHours: 8.5m,
            workEnvironment: WorkEnvironment.Office,
            contactLensUsage: ContactLensUsage.Daily,
            lifestyleNotes: "Works on computer all day");

        patient.Email.Should().Be("nguyenvanb@gmail.com");
        patient.Occupation.Should().Be("Software Engineer");
        patient.OcularHistory.Should().Be("Near-sightedness since age 12");
        patient.SystemicHistory.Should().Be("None");
        patient.CurrentMedications.Should().Be("Vitamin A");
        patient.ScreenTimeHours.Should().Be(8.5m);
        patient.WorkEnvironment.Should().Be(WorkEnvironment.Office);
        patient.ContactLensUsage.Should().Be(ContactLensUsage.Daily);
        patient.LifestyleNotes.Should().Be("Works on computer all day");
    }

    [Fact]
    public void UpdateIntake_WithNullOptionalFields_IsValid()
    {
        var patient = CreateMedicalPatient();

        patient.UpdateIntake(
            fullName: "Nguyen Van B",
            phone: "0901234567",
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Male,
            address: null,
            cccd: null,
            email: null,
            occupation: null,
            ocularHistory: null,
            systemicHistory: null,
            currentMedications: null,
            screenTimeHours: null,
            workEnvironment: null,
            contactLensUsage: null,
            lifestyleNotes: null);

        patient.Email.Should().BeNull();
        patient.Occupation.Should().BeNull();
        patient.ScreenTimeHours.Should().BeNull();
    }

    [Fact]
    public void UpdateIntake_PreservesOriginalFields()
    {
        var patient = CreateMedicalPatient();

        patient.UpdateIntake(
            fullName: "Updated Name",
            phone: "0909999999",
            dateOfBirth: new DateTime(1985, 6, 15),
            gender: Gender.Female,
            address: "456 Le Loi",
            cccd: "098765432101",
            email: "updated@email.com",
            occupation: null,
            ocularHistory: null,
            systemicHistory: null,
            currentMedications: null,
            screenTimeHours: null,
            workEnvironment: null,
            contactLensUsage: null,
            lifestyleNotes: null);

        patient.FullName.Should().Be("Updated Name");
        patient.Phone.Should().Be("0909999999");
        patient.DateOfBirth.Should().Be(new DateTime(1985, 6, 15));
        patient.Gender.Should().Be(Gender.Female);
        patient.Address.Should().Be("456 Le Loi");
        patient.Cccd.Should().Be("098765432101");
    }
}

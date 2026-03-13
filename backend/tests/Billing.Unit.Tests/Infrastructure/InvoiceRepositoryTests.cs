using FluentAssertions;

namespace Billing.Unit.Tests.Infrastructure;

public class InvoiceRepositoryTests
{
    private static readonly string InfrastructurePath = Path.GetFullPath(
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Modules", "Billing", "Billing.Infrastructure"));

    [Fact]
    public void GetNextInvoiceNumberAsync_UsesSequence_NotMaxPlusOne()
    {
        // Verify the repository source code uses SQL SEQUENCE pattern
        var sourceFile = Path.Combine(InfrastructurePath, "Repositories", "InvoiceRepository.cs");

        File.Exists(sourceFile).Should().BeTrue($"expected source file at {sourceFile}");

        var source = File.ReadAllText(sourceFile);

        source.Should().Contain("NEXT VALUE FOR",
            "GetNextInvoiceNumberAsync should use SQL SEQUENCE for thread-safe invoice numbering");
        source.Should().NotContain("OrderByDescending(i => i.InvoiceNumber)",
            "GetNextInvoiceNumberAsync should not use MAX+1 pattern which has race conditions");
    }

    [Fact]
    public void InvoiceNumber_SequenceMigration_Exists()
    {
        // Verify that a migration file exists containing the SQL SEQUENCE creation
        var migrationsDir = Path.Combine(InfrastructurePath, "Migrations");

        Directory.Exists(migrationsDir).Should().BeTrue($"expected migrations directory at {migrationsDir}");

        var migrationFiles = Directory.GetFiles(migrationsDir, "*.cs")
            .Where(f => !f.EndsWith(".Designer.cs") && !f.EndsWith("Snapshot.cs"))
            .ToList();

        var sequenceMigration = migrationFiles
            .Where(f => File.ReadAllText(f).Contains("InvoiceNumberSeq"))
            .ToList();

        sequenceMigration.Should().NotBeEmpty(
            "A migration should exist creating the InvoiceNumberSeq SQL SEQUENCE");

        var migrationContent = File.ReadAllText(sequenceMigration.First());

        migrationContent.Should().Contain("CREATE SEQUENCE",
            "Migration should create a SQL SEQUENCE");
        migrationContent.Should().Contain("billing.InvoiceNumberSeq",
            "Sequence should be in the billing schema");
    }

    [Fact]
    public void InvoiceNumber_UniqueIndex_ExistsInMigration()
    {
        // Verify that a unique index on InvoiceNumber exists in the initial migration
        var migrationsDir = Path.Combine(InfrastructurePath, "Migrations");
        var initialMigration = Directory.GetFiles(migrationsDir, "*InitialBilling.cs")
            .FirstOrDefault();

        initialMigration.Should().NotBeNull("Initial billing migration should exist");

        var content = File.ReadAllText(initialMigration!);
        content.Should().Contain("IX_Invoices_InvoiceNumber",
            "A unique index on InvoiceNumber should exist as safety net against duplicates");
    }
}

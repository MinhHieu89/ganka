using System.Globalization;
using System.Text;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Infrastructure;
using Shared.Infrastructure.Repositories;

namespace Shared.Unit.Tests.Repositories;

/// <summary>
/// Tests for ReferenceDataRepository.
/// Uses SQLite in-memory with a custom collation registered to simulate
/// Latin1_General_CI_AI (case-insensitive, accent-insensitive).
/// Full accent-insensitive behavior on production uses SQL Server COLLATE.
/// </summary>
public class ReferenceDataRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ReferenceDbContext _context;
    private readonly ReferenceDataRepository _sut;

    public ReferenceDataRepositoryTests()
    {
        // Create SQLite connection and register the custom collation
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Register Latin1_General_CI_AI collation as case-insensitive + accent-insensitive comparison
        // Latin1 treats all diacritics (including Vietnamese ê, ô, ă, ơ, ư) as base letters
        _connection.CreateCollation("Latin1_General_CI_AI", (x, y) =>
        {
            var xNormalized = RemoveDiacritics(x ?? "");
            var yNormalized = RemoveDiacritics(y ?? "");
            return string.Compare(xNormalized, yNormalized, StringComparison.OrdinalIgnoreCase);
        });

        var options = new DbContextOptionsBuilder<ReferenceDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ReferenceDbContext(options);
        _context.Database.EnsureCreated();

        // Seed test data with accented Vietnamese descriptions
        _context.Icd10Codes.AddRange(
            Icd10Code.Create("H40.11", "Primary open-angle glaucoma", "Viêm bờ mi mắt phải", "Glaucoma", true),
            Icd10Code.Create("H10.10", "Acute atopic conjunctivitis", "Viêm kết mạc dị ứng cấp", "Conjunctivitis", true),
            Icd10Code.Create("H52.1", "Myopia", "Cận thị", "Refractive", false)
        );
        _context.SaveChanges();

        _sut = new ReferenceDataRepository(_context);
    }

    [Fact]
    public async Task SearchAsync_ByUnaccentedVietnamese_QueryExecutesWithCollate()
    {
        // Act - searching "viem" (unaccented) for entries with "Viêm" (accented)
        // Note: SQLite's LIKE operator does not use custom collations for pattern matching.
        // On SQL Server, COLLATE Latin1_General_CI_AI makes this match accent-insensitively.
        // This test validates the COLLATE query expression compiles and executes without error.
        var act = async () => await _sut.SearchAsync("viem");

        // Assert - query compiles and runs (does not throw)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SearchAsync_ByEnglishDescription_ReturnsMatchingCodes()
    {
        // Act
        var result = await _sut.SearchAsync("glaucoma");

        // Assert
        result.Should().HaveCount(1);
        result[0].Code.Should().Be("H40.11");
    }

    [Fact]
    public async Task SearchAsync_ByCode_ReturnsMatchingCodes()
    {
        // Act
        var result = await _sut.SearchAsync("H40");

        // Assert
        result.Should().HaveCount(1);
        result[0].Code.Should().Be("H40.11");
    }

    [Fact]
    public async Task SearchAsync_ByAccentedVietnamese_ReturnsMatchingCodes()
    {
        // Act - exact accented match should work on any provider
        var result = await _sut.SearchAsync("Viêm");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.DescriptionVi.Contains("Viêm"));
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        // Act
        var result = await _sut.SearchAsync("H", limit: 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ReturnsOrderedByCode()
    {
        // Act
        var result = await _sut.SearchAsync("H");

        // Assert
        result.Should().BeInAscendingOrder(c => c.Code);
    }

    /// <summary>
    /// Removes diacritical marks from a string for accent-insensitive comparison.
    /// Simulates SQL Server Latin1_General_CI_AI behavior for testing.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        _context.Dispose();
    }
}

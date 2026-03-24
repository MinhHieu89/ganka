using FluentAssertions;

namespace Ganka28.ArchitectureTests;

/// <summary>
/// Architecture tests that verify all API endpoint files have proper permission
/// enforcement via RequirePermissions calls using Permissions.* constants.
///
/// These tests scan source files directly (not compiled assemblies) to ensure
/// every non-public endpoint file includes at least one RequirePermissions call.
/// </summary>
public class PermissionEnforcementTests
{
    /// <summary>
    /// Endpoint files that are intentionally public (no auth required).
    /// </summary>
    private static readonly HashSet<string> ExcludedFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "PublicBookingEndpoints.cs",
        "PublicOsdiEndpoints.cs"
    };

    /// <summary>
    /// Resolves the backend/src directory by navigating up from the test assembly location.
    /// </summary>
    private static string GetBackendSrcDirectory()
    {
        // Test runs from bin/Debug/net10.0/ — navigate up to repo root
        var assemblyDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(assemblyDir);

        // Walk up until we find the backend directory
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "src")))
        {
            dir = dir.Parent;
        }

        dir.Should().NotBeNull("Could not find repo root containing backend/src from {0}", assemblyDir);
        return Path.Combine(dir!.FullName, "backend", "src");
    }

    /// <summary>
    /// Finds all *Endpoints.cs files under backend/src/ that are not in the exclusion list.
    /// </summary>
    private static List<(string FilePath, string FileName)> GetNonPublicEndpointFiles()
    {
        var srcDir = GetBackendSrcDirectory();
        var endpointFiles = Directory.GetFiles(srcDir, "*Endpoints.cs", SearchOption.AllDirectories);

        return endpointFiles
            .Select(f => (FilePath: f, FileName: Path.GetFileName(f)))
            .Where(f => !ExcludedFiles.Contains(f.FileName))
            .ToList();
    }

    [Fact]
    public void AllNonPublicEndpointFiles_MustContain_RequirePermissionsCalls()
    {
        // Arrange
        var endpointFiles = GetNonPublicEndpointFiles();
        endpointFiles.Should().NotBeEmpty("there should be at least one non-public endpoint file");

        var filesWithoutPermissions = new List<string>();

        // Act
        foreach (var (filePath, fileName) in endpointFiles)
        {
            var content = File.ReadAllText(filePath);
            if (!content.Contains("RequirePermissions(Permissions."))
            {
                filesWithoutPermissions.Add(fileName);
            }
        }

        // Assert — this test is expected to FAIL (RED) until Wave 1 adds permissions
        filesWithoutPermissions.Should().BeEmpty(
            "all non-public endpoint files must call RequirePermissions(Permissions.*) " +
            "but the following files are missing permission enforcement: {0}",
            string.Join(", ", filesWithoutPermissions));
    }

    [Fact]
    public void NoEndpointFile_ShouldUse_StringLiteralPermissions()
    {
        // Arrange
        var endpointFiles = GetNonPublicEndpointFiles();
        endpointFiles.Should().NotBeEmpty("there should be at least one non-public endpoint file");

        var filesWithStringLiterals = new List<string>();

        // Act
        foreach (var (filePath, fileName) in endpointFiles)
        {
            var content = File.ReadAllText(filePath);
            // Check for RequirePermissions("..." — string literal instead of Permissions.* constant
            if (content.Contains("RequirePermissions(\""))
            {
                filesWithStringLiterals.Add(fileName);
            }
        }

        // Assert
        filesWithStringLiterals.Should().BeEmpty(
            "endpoint files must use Permissions.* constants, not string literals. " +
            "Files using string literals: {0}",
            string.Join(", ", filesWithStringLiterals));
    }
}

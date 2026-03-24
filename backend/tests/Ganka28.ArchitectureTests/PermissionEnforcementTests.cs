using FluentAssertions;

namespace Ganka28.ArchitectureTests;

/// <summary>
/// Architecture tests that verify all API endpoint files have proper
/// permission enforcement via RequirePermissions() calls.
/// Created as part of 11-00 (RED) and satisfied by 11-01 (GREEN).
/// </summary>
public class PermissionEnforcementTests
{
    /// <summary>
    /// Files that are intentionally public and should NOT have RequirePermissions.
    /// </summary>
    private static readonly string[] ExcludedFiles =
    [
        "PublicBookingEndpoints.cs",
        "PublicOsdiEndpoints.cs"
    ];

    private static string GetBackendSrcPath()
    {
        // Navigate from bin/Debug/net10.0 up to the repo root, then into backend/src
        var assemblyDir = Path.GetDirectoryName(typeof(PermissionEnforcementTests).Assembly.Location)!;
        // Path: bin/Debug/net10.0 -> (3 up) Ganka28.ArchitectureTests -> (1 up) tests -> (1 up) backend
        var backendDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));
        return Path.Combine(backendDir, "src");
    }

    private static IEnumerable<string> GetEndpointFiles()
    {
        var srcPath = GetBackendSrcPath();
        var modulesPath = Path.Combine(srcPath, "Modules");
        var sharedPath = Path.Combine(srcPath, "Shared");

        var files = new List<string>();
        if (Directory.Exists(modulesPath))
            files.AddRange(Directory.GetFiles(modulesPath, "*ApiEndpoints.cs", SearchOption.AllDirectories));
        if (Directory.Exists(sharedPath))
            files.AddRange(Directory.GetFiles(sharedPath, "*ApiEndpoints.cs", SearchOption.AllDirectories));

        return files;
    }

    [Fact]
    public void AllNonPublicEndpointFiles_MustContain_RequirePermissionsCalls()
    {
        var endpointFiles = GetEndpointFiles().ToList();

        endpointFiles.Should().NotBeEmpty("there should be endpoint files in the codebase");

        var failures = new List<string>();

        foreach (var file in endpointFiles)
        {
            var fileName = Path.GetFileName(file);
            if (ExcludedFiles.Contains(fileName))
                continue;

            var content = File.ReadAllText(file);
            if (!content.Contains("RequirePermissions(Permissions."))
            {
                failures.Add(fileName);
            }
        }

        failures.Should().BeEmpty(
            "all non-public endpoint files must contain RequirePermissions(Permissions.*) calls, " +
            $"but these files are missing them: {string.Join(", ", failures)}");
    }

    [Fact]
    public void NoEndpointFile_ShouldUse_StringLiteralPermissions()
    {
        var endpointFiles = GetEndpointFiles().ToList();

        endpointFiles.Should().NotBeEmpty("there should be endpoint files in the codebase");

        var violations = new List<string>();

        foreach (var file in endpointFiles)
        {
            var content = File.ReadAllText(file);
            // Check for RequirePermissions("..." -- string literal instead of Permissions.* constant
            if (content.Contains("RequirePermissions(\""))
            {
                violations.Add(Path.GetFileName(file));
            }
        }

        violations.Should().BeEmpty(
            "no endpoint file should use string literal permissions -- use Permissions.* constants instead, " +
            $"but these files have string literals: {string.Join(", ", violations)}");
    }
}

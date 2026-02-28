using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Ganka28.ArchitectureTests;

/// <summary>
/// Tests that enforce module boundary rules in the modular monolith.
/// Each module's Domain, Application, Contracts, and Infrastructure layers
/// must respect isolation constraints to prevent coupling.
/// </summary>
public class ModuleBoundaryTests
{
    /// <summary>
    /// All module names in the system. Used for data-driven boundary tests.
    /// </summary>
    private static readonly string[] ModuleNames =
    [
        "Auth",
        "Audit",
        "Patient",
        "Clinical",
        "Scheduling",
        "Pharmacy",
        "Optical",
        "Billing",
        "Treatment"
    ];

    /// <summary>
    /// Returns other module namespaces (excluding Shared.*) for cross-module reference checks.
    /// </summary>
    private static string[] GetOtherModuleNamespaces(string currentModule)
    {
        return ModuleNames
            .Where(m => m != currentModule)
            .SelectMany(m => new[]
            {
                $"{m}.Domain",
                $"{m}.Contracts",
                $"{m}.Application",
                $"{m}.Infrastructure"
            })
            .ToArray();
    }

    /// <summary>
    /// Returns other module internal namespaces (Domain, Application, Infrastructure - not Contracts).
    /// Contracts is the public face and can be referenced.
    /// </summary>
    private static string[] GetOtherModuleInternalNamespaces(string currentModule)
    {
        return ModuleNames
            .Where(m => m != currentModule)
            .SelectMany(m => new[]
            {
                $"{m}.Domain",
                $"{m}.Application",
                $"{m}.Infrastructure"
            })
            .ToArray();
    }

    /// <summary>
    /// Gets the assembly for a given module layer.
    /// Returns null if the assembly cannot be loaded (scaffold-only modules).
    /// </summary>
    private static Assembly? GetAssembly(string moduleName, string layer)
    {
        var assemblyName = $"{moduleName}.{layer}";
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            return null;
        }
    }

    public static IEnumerable<object[]> ModuleData =>
        ModuleNames.Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(ModuleData))]
    public void Domain_Should_Not_Reference_Other_Modules(string moduleName)
    {
        // Arrange
        var assembly = GetAssembly(moduleName, "Domain");
        if (assembly is null) return; // Skip scaffold-only modules

        var otherModuleNamespaces = GetOtherModuleNamespaces(moduleName);

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining($"{moduleName}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(otherModuleNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}.Domain should not reference other modules. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(ModuleData))]
    public void Domain_Should_Not_Reference_Infrastructure(string moduleName)
    {
        // Arrange
        var assembly = GetAssembly(moduleName, "Domain");
        if (assembly is null) return;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining($"{moduleName}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                $"{moduleName}.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "System.Net.Http")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}.Domain should not reference Infrastructure or EF Core. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(ModuleData))]
    public void Application_Should_Not_Reference_Other_Module_Internals(string moduleName)
    {
        // Arrange
        var assembly = GetAssembly(moduleName, "Application");
        if (assembly is null) return;

        var otherModuleInternals = GetOtherModuleInternalNamespaces(moduleName);

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining($"{moduleName}.Application")
            .ShouldNot()
            .HaveDependencyOnAny(otherModuleInternals)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}.Application should not reference other modules' internals " +
            $"(only Contracts and Shared.* are allowed). " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(ModuleData))]
    public void Contracts_Should_Not_Reference_Module_Internals(string moduleName)
    {
        // Arrange
        var assembly = GetAssembly(moduleName, "Contracts");
        if (assembly is null) return;

        // Contracts should not reference its own module's Domain, Application, or Infrastructure
        var forbiddenNamespaces = new[]
        {
            $"{moduleName}.Domain",
            $"{moduleName}.Application",
            $"{moduleName}.Infrastructure"
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining($"{moduleName}.Contracts")
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{moduleName}.Contracts should not reference its own module internals. " +
            $"Contracts must be self-contained. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(ModuleData))]
    public void No_Module_Should_Reference_Bootstrapper(string moduleName)
    {
        // Arrange -- check all 4 layers
        var layers = new[] { "Domain", "Contracts", "Application", "Infrastructure" };

        foreach (var layer in layers)
        {
            var assembly = GetAssembly(moduleName, layer);
            if (assembly is null) continue;

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny("Bootstrapper")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.{layer} should not reference the Bootstrapper assembly. " +
                $"Violating types: {FormatFailingTypes(result)}");
        }
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.IsSuccessful || result.FailingTypes is null)
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}

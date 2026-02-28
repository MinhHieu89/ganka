using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Ganka28.ArchitectureTests;

/// <summary>
/// Tests that verify the 4-layer dependency direction is correct.
/// The dependency arrow must always point inward:
///   Infrastructure -> Application -> Domain
///   Contracts stands alone (references only Shared.Contracts)
///
/// These tests analyze ALL module assemblies to verify the pattern
/// is consistently applied across the entire codebase.
/// </summary>
public class DependencyDirectionTests
{
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
    /// Loads all assemblies for a given layer across all modules.
    /// </summary>
    private static Assembly[] GetLayerAssemblies(string layer)
    {
        return ModuleNames
            .Select(m =>
            {
                try { return Assembly.Load($"{m}.{layer}"); }
                catch { return null; }
            })
            .Where(a => a is not null)
            .Cast<Assembly>()
            .ToArray();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        // Arrange
        var domainAssemblies = GetLayerAssemblies("Domain");
        domainAssemblies.Should().NotBeEmpty("at least one Domain assembly should exist");

        // Act & Assert
        foreach (var assembly in domainAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Domain", "");
            var applicationNamespaces = ModuleNames
                .Select(m => $"{m}.Application")
                .Concat(["Shared.Application"])
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(applicationNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Domain should not depend on any Application layer. " +
                $"Violating types: {FormatFailingTypes(result)}");
        }
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var domainAssemblies = GetLayerAssemblies("Domain");
        domainAssemblies.Should().NotBeEmpty();

        // Act & Assert
        foreach (var assembly in domainAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Domain", "");
            var infrastructureNamespaces = ModuleNames
                .Select(m => $"{m}.Infrastructure")
                .Concat(["Shared.Infrastructure"])
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(infrastructureNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Domain should not depend on any Infrastructure layer. " +
                $"Violating types: {FormatFailingTypes(result)}");
        }
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var applicationAssemblies = GetLayerAssemblies("Application");
        applicationAssemblies.Should().NotBeEmpty();

        // Act & Assert
        foreach (var assembly in applicationAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Application", "");
            var infrastructureNamespaces = ModuleNames
                .Select(m => $"{m}.Infrastructure")
                .Concat(["Shared.Infrastructure"])
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(infrastructureNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Application should not depend on any Infrastructure layer. " +
                $"Violating types: {FormatFailingTypes(result)}");
        }
    }

    [Fact]
    public void Contracts_Should_Be_Independent()
    {
        // Arrange
        var contractsAssemblies = GetLayerAssemblies("Contracts");
        contractsAssemblies.Should().NotBeEmpty();

        // Act & Assert
        foreach (var assembly in contractsAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Contracts", "");

            // Contracts should not depend on Domain, Application, or Infrastructure
            // Exception: Shared.Contracts is allowed (filtered out below)
            var forbiddenNamespaces = ModuleNames
                .SelectMany(m => new[]
                {
                    $"{m}.Domain",
                    $"{m}.Application",
                    $"{m}.Infrastructure"
                })
                .Concat(["Shared.Domain", "Shared.Application", "Shared.Infrastructure"])
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Contracts should be independent of Domain, Application, and Infrastructure. " +
                $"Only Shared.Contracts references are allowed. " +
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

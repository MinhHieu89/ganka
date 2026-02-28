using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Ganka28.ArchitectureTests;

/// <summary>
/// Tests that verify the 5-layer dependency direction is correct.
/// The dependency arrow must always point inward:
///   Presentation -> Application -> Domain
///   Infrastructure -> Application -> Domain
///   Contracts stands alone (references only Shared.Contracts)
///   Presentation must NOT depend on Infrastructure
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
    public void Presentation_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var presentationAssemblies = GetLayerAssemblies("Presentation");

        // Skip if no Presentation assemblies exist yet (scaffold modules don't have them)
        if (presentationAssemblies.Length == 0)
            return;

        // Act & Assert
        foreach (var assembly in presentationAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Presentation", "");
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
                $"{moduleName}.Presentation should not depend on any Infrastructure layer. " +
                $"Presentation depends only on Application. " +
                $"Violating types: {FormatFailingTypes(result)}");
        }
    }

    [Fact]
    public void Presentation_Should_Not_Depend_On_Domain_Directly()
    {
        // Arrange
        var presentationAssemblies = GetLayerAssemblies("Presentation");

        // Skip if no Presentation assemblies exist yet
        if (presentationAssemblies.Length == 0)
            return;

        // Act & Assert
        foreach (var assembly in presentationAssemblies)
        {
            var moduleName = assembly.GetName().Name!.Replace(".Presentation", "");

            // Presentation should go through Application, not Domain directly.
            // Shared.Domain is excluded because it contains cross-cutting primitives
            // (Result<T>, Error, BranchId) that Presentation needs for HTTP response mapping.
            // Module-specific Domain layers (Auth.Domain, etc.) must NOT be referenced.
            var domainNamespaces = ModuleNames
                .Select(m => $"{m}.Domain")
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(domainNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Presentation should not depend on any Domain layer directly. " +
                $"Use Application-layer DTOs and interfaces instead. " +
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

            // Contracts should not depend on Domain, Application, Infrastructure, or Presentation
            // Exception: Shared.Contracts is allowed (filtered out below)
            var forbiddenNamespaces = ModuleNames
                .SelectMany(m => new[]
                {
                    $"{m}.Domain",
                    $"{m}.Application",
                    $"{m}.Infrastructure",
                    $"{m}.Presentation"
                })
                .Concat(["Shared.Domain", "Shared.Application", "Shared.Infrastructure"])
                .ToArray();

            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName}.Contracts should be independent of Domain, Application, Infrastructure, and Presentation. " +
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

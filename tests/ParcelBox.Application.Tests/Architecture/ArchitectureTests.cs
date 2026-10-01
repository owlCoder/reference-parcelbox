using System.Reflection;
using ParcelBox.Application.Services;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests.Architecture;

public sealed class ArchitectureTests
{
    [Fact]
    public void InterfaceNamespaces_ContainOnlyInterfaces()
    {
        var invalidTypes = typeof(ParcelService).Assembly
            .GetTypes()
            .Where(type =>
                type.IsPublic &&
                type.Namespace?.StartsWith("ParcelBox.Application.Interfaces", StringComparison.Ordinal) == true &&
                !type.IsInterface)
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(invalidTypes);
    }

    [Fact]
    public void ApplicationEnums_AreDeclaredInEnumsNamespace()
    {
        var invalidTypes = typeof(ParcelService).Assembly
            .GetTypes()
            .Where(type =>
                type.IsPublic &&
                type.IsEnum &&
                type.Namespace != "ParcelBox.Application.Enums")
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(invalidTypes);
    }

    [Fact]
    public void Services_DependOnAbstractions()
    {
        var invalidParameters = typeof(ParcelService).Assembly
            .GetTypes()
            .Where(type =>
                type.IsPublic &&
                type.IsClass &&
                type.Namespace == "ParcelBox.Application.Services")
            .SelectMany(type => type.GetConstructors())
            .SelectMany(constructor => constructor.GetParameters())
            .Where(parameter =>
                !parameter.ParameterType.IsInterface &&
                parameter.ParameterType != typeof(TimeProvider))
            .Select(parameter =>
                $"{parameter.Member.DeclaringType?.Name}.{parameter.Name}: {parameter.ParameterType.Name}")
            .ToArray();

        Assert.Empty(invalidParameters);
    }

    [Fact]
    public void DomainModels_AreStateOnlyAndUseModelsNamespace()
    {
        var modelTypes = typeof(Compartment).Assembly
            .GetTypes()
            .Where(type => type.IsPublic && type.IsClass)
            .ToArray();

        Assert.All(modelTypes, type => Assert.EndsWith(".Models", type.Namespace));

        var businessMethods = modelTypes
            .SelectMany(type => type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly))
            .Where(method => !method.IsSpecialName)
            .Select(method => $"{method.DeclaringType?.Name}.{method.Name}")
            .ToArray();

        Assert.Empty(businessMethods);
    }

    [Fact]
    public void DomainEnums_UseEnumsNamespace()
    {
        var invalidEnums = typeof(Compartment).Assembly
            .GetTypes()
            .Where(type =>
                type.IsPublic &&
                type.IsEnum &&
                !string.Equals(type.Namespace, "ParcelBox.Domain.Common.Enums", StringComparison.Ordinal) &&
                type.Namespace?.EndsWith(".Enums", StringComparison.Ordinal) != true)
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(invalidEnums);
    }
}

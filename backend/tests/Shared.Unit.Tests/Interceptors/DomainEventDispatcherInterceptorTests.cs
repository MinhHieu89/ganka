using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Shared.Domain;
using Shared.Infrastructure.Interceptors;
using Wolverine;

namespace Shared.Unit.Tests.Interceptors;

public class DomainEventDispatcherInterceptorTests
{
    [Fact]
    public void Constructor_AcceptsIMessageBus_NotIServiceProvider()
    {
        // Arrange & Act
        var messageBus = Substitute.For<IMessageBus>();
        var interceptor = new DomainEventDispatcherInterceptor(messageBus);

        // Assert
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void Interceptor_ShouldNotHaveServiceProviderField()
    {
        // The interceptor should not contain any IServiceProvider field,
        // confirming it uses direct IMessageBus injection
        var type = typeof(DomainEventDispatcherInterceptor);
        var fields = type.GetFields(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        fields.Should().NotContain(f => f.FieldType == typeof(IServiceProvider),
            "interceptor should not depend on IServiceProvider");
    }

    [Fact]
    public void Interceptor_ShouldHaveIMessageBusField()
    {
        var type = typeof(DomainEventDispatcherInterceptor);
        var fields = type.GetFields(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        fields.Should().Contain(f => f.FieldType == typeof(IMessageBus),
            "interceptor should inject IMessageBus directly");
    }
}

using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.di;
using jjm.one.CommandLineToolWrapper.settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace jjm.one.CommandLineToolWrapper.Tests.di;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddToolWrapper_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var toolSettings = new ToolSettings();
        var wrapperSettings = new WrapperSettings();

        // Act
        services.AddToolWrapper(toolSettings, wrapperSettings);

        // Assert
        services.Should().ContainSingle(s => s.ServiceType == typeof(ILogger<ToolWrapper>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(s => s.ServiceType == typeof(IProcessRunner) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(s => s.ServiceType == typeof(IToolWrapper) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(s => s.ServiceType == typeof(ToolSettings) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(s => s.ServiceType == typeof(WrapperSettings) && s.Lifetime == ServiceLifetime.Singleton);
    }
}
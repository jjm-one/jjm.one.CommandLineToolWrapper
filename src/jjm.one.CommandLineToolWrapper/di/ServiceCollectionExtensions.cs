using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace jjm.one.CommandLineToolWrapper.di;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an instance of the ToolWrapper and all its dependencies to the provided IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the ToolWrapper to.</param>
    /// <param name="toolSettings">The settings for the tool that the ToolWrapper will use.</param>
    /// <param name="wrapperSettings">The settings for the ToolWrapper itself.</param>
    /// <returns>The same IServiceCollection provided as a parameter, to allow for chaining calls.</returns>
    public static IServiceCollection AddToolWrapper(this IServiceCollection services, ToolSettings toolSettings, WrapperSettings wrapperSettings)
    {
        // Add the logger instances to the service collection.
        services.AddSingleton<ILogger<ToolWrapper>, Logger<ToolWrapper>>();
        
        // Add the necessary settings instances to service collection.
        services.AddSingleton(toolSettings);
        services.AddSingleton(wrapperSettings);
        
        // Add the backend process runner instance to the service collection.
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        
        // Add the instance of the tool wrapper.
        services.AddSingleton<IToolWrapper, ToolWrapper>();
        
        return services;
    }
}
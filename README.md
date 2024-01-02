# jjm.one.CommandLineToolWrapper

A C# library that provides a wrapper for command line tools.

## Status

|                       |                       |
|----------------------:|-----------------------|
| Build & Test Status (main) | [![Build&Test](https://github.com/jjm-one/jjm.one.CommandLineToolWrapper/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jjm-one/jjm.one.CommandLineToolWrapper/actions/workflows/dotnet.yml) |
| Nuget Package Version | [![Nuget Version](https://img.shields.io/nuget/v/jjm.one.CommandLineToolWrapper?style=flat-square)](https://www.nuget.org/packages/jjm.one.CommandLineToolWrapper/) |
| SonarCloudQuality Gate Status | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jjm-one_jjm.one.CommandLineToolWrapper&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jjm-one_jjm.one.CommandLineToolWrapper) |

## Table of contents

- [jjm.one.CommandLineToolWrapper](#jjmonecommandlinetoolwrapper)
  - [Status](#status)
  - [Table of contents](#table-of-contents)
  - [Brief overview of the interfaces and classes](#brief-overview-of-the-interfaces-and-classes)
    - [`IToolWrapper` Interface](#itoolwrapper-interface)
    - [`ToolWrapper` Class](#toolwrapper-class)
    - [`ProcessResult` Class](#processresult-class)
    - [`ToolWrapper` Constructor](#toolwrapper-constructor)
    - [`ToolWrapper.RunCommandAsync` Method](#toolwrapperruncommandasync-method)
  - [Dependency Injection](#dependency-injection)

## Brief overview of the interfaces and classes

All interfaces and classes provided by this package are briefly presented below.

### `IToolWrapper` Interface

This interface defines a contract for a wrapper around a command line tool. It has a single method, `RunCommandAsync`, which takes a command and its arguments, and returns a `Task` that completes with a `ProcessResult`.

```csharp
public interface IToolWrapper
{
    Task<ProcessResult> RunCommandAsync(string command, string arguments);
}
```

### `ToolWrapper` Class

This class is the default implementation of the `IToolWrapper` interface. It uses an instance of `IProcessRunner` to execute commands, and logs information about the tool's operations. It also has settings for the command line tool and the wrapper itself, which are provided through the constructor.

```csharp
public class ToolWrapper : IToolWrapper
{
    public ToolWrapper(ToolSettings toolSettings, WrapperSettings wrapperSettings, IProcessRunner? processRunner = null, ILogger<ToolWrapper>? logger = null)
    {
        // ...
    }

    public Task<ProcessResult> RunCommandAsync(string command, string arguments)
    {
        // ...
    }
}
```

You can use the `ToolWrapper` class to run a command like this:

```csharp
var toolWrapper = new ToolWrapper(new ToolSettings(), new WrapperSettings());
var result = await toolWrapper.RunCommandAsync("echo", "Hello, world!");
Console.WriteLine(result.Output);  // Outputs: Hello, world!
```

### `ProcessResult` Class

This class represents the result of a process run by the `ToolWrapper`. It contains the exit code of the process and optionally, the output and error messages.

```csharp
public class ProcessResult
{
    public int ExitCode { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}
```

When you run a command with the `ToolWrapper`, you get a `ProcessResult`:

```csharp
var result = await toolWrapper.RunCommandAsync("echo", "Hello, world!");
Console.WriteLine(result.ExitCode);  // Outputs: 0
Console.WriteLine(result.Output);  // Outputs: Hello, world!
Console.WriteLine(result.Error);  // Outputs: null
```

### `ToolWrapper` Constructor

The constructor of the `ToolWrapper` class takes in `ToolSettings` and `WrapperSettings` for the command line tool and the wrapper respectively. It also takes an optional `IProcessRunner` and `ILogger<ToolWrapper>`. If no `IProcessRunner` is provided, a new `ProcessRunner` is created. If no `ILogger<ToolWrapper>` is provided, logging is disabled.

```csharp
public ToolWrapper(ToolSettings toolSettings, WrapperSettings wrapperSettings, IProcessRunner? processRunner = null, ILogger<ToolWrapper>? logger = null)
{
    // ...
}
```

### `ToolWrapper.RunCommandAsync` Method

This method is the implementation of the `RunCommandAsync` method from the `IToolWrapper` interface. It runs a command asynchronously and returns a `ProcessResult`.

```csharp
public Task<ProcessResult> RunCommandAsync(string command, string arguments)
{
    // ...
}
```

You can use this method to run a command like this:

```csharp
var result = await toolWrapper.RunCommandAsync("echo", "Hello, world!");
Console.WriteLine(result.Output);  // Outputs: Hello, world!
```

## Dependency Injection

The `AddToolWrapper` method is an extension method for `IServiceCollection` that you can use to register the `ToolWrapper` class and its dependencies in the dependency injection container.

Here's an example of how you can use it in the `ConfigureServices` method of your `Startup.cs` file:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Create instances of ToolSettings and WrapperSettings
    // You might want to populate these from your configuration
    var toolSettings = new ToolSettings();
    var wrapperSettings = new WrapperSettings();

    // Use the AddToolWrapper method to add ToolWrapper and its dependencies
    services.AddToolWrapper(toolSettings, wrapperSettings);

    // Add other services...
}
```

In this example, `ToolSettings` and `WrapperSettings` are being created as new instances. You might want to populate these from your application's configuration (e.g., from an `appsettings.json` file) instead.

Please replace `YourNamespace` with the actual namespace where the `AddToolWrapper` method is defined.

After you've registered `ToolWrapper` with the `IServiceCollection`, you can have it automatically injected into your classes by adding a parameter of type `IToolWrapper` to the constructor of your class:

```csharp
public class MyClass
{
    private readonly IToolWrapper _toolWrapper;

    public MyClass(IToolWrapper toolWrapper)
    {
        _toolWrapper = toolWrapper;
    }

    // Use _toolWrapper in your methods...
}
```

In this example, `MyClass` has a dependency on `IToolWrapper`, which will be automatically injected by the .NET Core dependency injection framework when `MyClass` is created.

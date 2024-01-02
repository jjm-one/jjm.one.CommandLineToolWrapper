using System;
using jjm.one.CommandLineToolWrapper.settings;

namespace jjm.one.CommandLineToolWrapper.Tests.settings;

public class WrapperSettingsTests
{
    [Fact]
    public void WrapperSettings_PropertiesAreInitializedWithExpectedValues()
    {
        // Arrange & Act
        var settings = new WrapperSettings();

        // Assert
        settings.WorkingDirectory.Should().Be(Environment.CurrentDirectory);
        settings.ErrorDialog.Should().BeFalse();
        settings.RetryCount.Should().Be(3);
        settings.RetryIntervalInSeconds.Should().Be(10);
        settings.RetryUseExitCodeAnalysis.Should().BeTrue();
        settings.RetryUseOutputAnalysis.Should().BeTrue();
        settings.RetryUseErrorAnalysis.Should().BeTrue();
    }
    
    [Fact]
    public void WrapperSettings_PropertiesAreInitializedWithCustomValues()
    {
        // Arrange & Act
        var settings = new WrapperSettings
        {
            WorkingDirectory = "Hello",
            ErrorDialog = true,
            RetryCount = 42,
            RetryIntervalInSeconds = 69,
            RetryUseErrorAnalysis = false,
            RetryUseOutputAnalysis = false,
            RetryUseExitCodeAnalysis = false
        };

        // Assert
        settings.WorkingDirectory.Should().Be("Hello");
        settings.ErrorDialog.Should().BeTrue();
        settings.RetryCount.Should().Be(42);
        settings.RetryIntervalInSeconds.Should().Be(69);
        settings.RetryUseExitCodeAnalysis.Should().BeFalse();
        settings.RetryUseOutputAnalysis.Should().BeFalse();
        settings.RetryUseErrorAnalysis.Should().BeFalse();
    }
}
using System.Collections.Generic;
using jjm.one.CommandLineToolWrapper.settings;

namespace jjm.one.CommandLineToolWrapper.Tests.settings;

public class ToolSettingsTests
{
    [Fact]
    public void ToolSettings_PropertiesAreInitializedWithExpectedValues()
    {
        // Arrange & Act
        var settings = new ToolSettings();

        // Assert
        settings.ToolPath.Should().Be(string.Empty);
        settings.CommandTemplates.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["help"] = "--help",
            ["version"] = "--version"
        });
        settings.RetryExitCodes.Should().BeEquivalentTo(new List<int> { 1 });
        settings.RetryOutputContains.Should().BeEquivalentTo(new List<string> { "network error", "timeout" });
        settings.RetryErrorContains.Should().BeEquivalentTo(new List<string> { "network error", "timeout" });
    }
    
    [Fact]
    public void ToolSettings_PropertiesAreInitializedWithCustomValues()
    {
        // Arrange & Act
        var settings = new ToolSettings
        {
            ToolPath = "Hello",
            CommandTemplates = new Dictionary<string, string>()
            {
                ["Hello"] = "Hello"
            },
            RetryExitCodes = [5],
            RetryErrorContains = ["Hello"],
            RetryOutputContains = ["Hello"]
        };

        // Assert
        settings.ToolPath.Should().Be("Hello");
        settings.CommandTemplates.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["Hello"] = "Hello"
        });
        settings.RetryExitCodes.Should().BeEquivalentTo(new List<int> { 5 });
        settings.RetryOutputContains.Should().BeEquivalentTo(new List<string> { "Hello" });
        settings.RetryErrorContains.Should().BeEquivalentTo(new List<string> { "Hello" });
    }
}
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.Tests.types;

public class ProcessResultTests
{
    [Fact]
    public void ProcessResult_PropertiesAreInitializedCorrectly()
    {
        // Arrange
        const int expectedExitCode = 0;
        const string expectedOutput = "output";
        const string expectedError = "error";

        // Act
        var result = new ProcessResult
        {
            ExitCode = expectedExitCode,
            Output = expectedOutput,
            Error = expectedError
        };

        // Assert
        result.ExitCode.Should().Be(expectedExitCode);
        result.Output.Should().Be(expectedOutput);
        result.Error.Should().Be(expectedError);
    }
}
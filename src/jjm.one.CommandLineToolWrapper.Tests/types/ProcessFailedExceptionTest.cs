using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.Tests.types;

public class ProcessFailedExceptionTest
{
    [Fact]
    public void ProcessFailedException_PropertiesAreInitializedCorrectly()
    {
        // Arrange
        const string expectedCommand = "command";
        const string expectedArguments = "arguments";
        var expectedProcessResult = new ProcessResult
        {
            ExitCode = 1,
            Output = "output",
            Error = "error"
        };

        // Act
        var exception = new ProcessFailedException(expectedCommand, expectedArguments, expectedProcessResult);

        // Assert
        exception.Command.Should().Be(expectedCommand);
        exception.Arguments.Should().Be(expectedArguments);
        exception.Result.Should().Be(expectedProcessResult);
    }
}
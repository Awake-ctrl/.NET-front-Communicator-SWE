using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;

using Communicator.Cloud.Logger;
using Communicator.Cloud.CloudFunction.FunctionLibrary;

namespace Communicator.Cloud.Logger.Tests;
public class LoggerTests : IDisposable
{
    private const string LogFile = "application.log";

    public LoggerTests()
    {
        try
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            File.Create(LogFile).Dispose();
        }
        catch (IOException)
        {
            // Ignore locks from other tests if running in parallel
        }
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }
        catch (IOException)
        {
            // Ignore locks if file is still in use by another process/test
        }
    }

    [Fact]
    public async Task InfoAsync_WritesToLocalFile()
    {
        var mockCloud = new Mock<ICloudFunctionLibrary>();
        var logger = new CloudLogger("TestModule", mockCloud.Object);
        await logger.InfoAsync("Hello Info");

        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("INFO", content);
        Assert.Contains("TestModule", content);
        Assert.Contains("Hello Info", content);
    }

    [Fact]
    public async Task WarnAsync_WritesToFile_AndCallsCloud()
    {
        var mockCloud = new Mock<ICloudFunctionLibrary>();
        mockCloud
            .Setup(m => m.SendLogAsync("TestModule", "WARNING", "Warn Test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = new CloudLogger("TestModule", mockCloud.Object);

        await logger.WarnAsync("Warn Test");
        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("WARNING", content);
        Assert.Contains("Warn Test", content);

        mockCloud.Verify(
            m => m.SendLogAsync("TestModule", "WARNING", "Warn Test", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ErrorAsync_MessageOnly_WritesToFile_AndCallsCloud()
    {
        var mockCloud = new Mock<ICloudFunctionLibrary>();
        mockCloud
            .Setup(m => m.SendLogAsync("TestModule", "ERROR", "Simple error message", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = new CloudLogger("TestModule", mockCloud.Object);

        await logger.ErrorAsync("Simple error message");

        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("SEVERE", content);
        Assert.Contains("Simple error message", content);

        mockCloud.Verify(
            m => m.SendLogAsync("TestModule", "ERROR", "Simple error message", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ErrorAsync_WithException_WritesToFile_AndCallsCloud()
    {
        var mockCloud = new Mock<ICloudFunctionLibrary>();
        mockCloud
            .Setup(m => m.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = new CloudLogger("TestModule", mockCloud.Object);
        Exception ex = new Exception("Something broke");

        await logger.ErrorAsync("Error happened", ex);
        string content = await File.ReadAllTextAsync(LogFile);

        Assert.Contains("SEVERE", content);
        Assert.Contains("Error happened", content);
        Assert.Contains("Something broke", content);

        mockCloud.Verify(m =>
            m.SendLogAsync("TestModule", "ERROR", It.Is<string>(msg => msg.Contains("Something broke")), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

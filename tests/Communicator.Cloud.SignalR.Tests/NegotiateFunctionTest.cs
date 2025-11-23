/******************************************************************************
* Filename    = NegotiateFunctionTests.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = UnitUnit test for NegotiateFunction Azure Function.
*****************************************************************************/

using System.Net;
using System.Text.Json;
using Communicator.Cloud.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Communicator.Cloud.SignalR.Tests;

/// <summary>
/// Class containing unit tests for the NegotiateFunction Azure Function.
/// </summary>
public class NegotiateFunctionTests
{
    // Mock objects and function instance
    private readonly Mock<ILogger<NegotiateFunction>> _mockLogger;
    private readonly Mock<FunctionContext> _mockContext;
    private readonly Mock<HttpRequestData> _mockRequest;
    private readonly NegotiateFunction _function;

    /// <summary>
    /// Constructor to set up the test environment.
    /// </summary>
    public NegotiateFunctionTests()
    {
        _mockLogger = new Mock<ILogger<NegotiateFunction>>();
        _mockContext = new Mock<FunctionContext>();

        IHost host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        IServiceProvider services = host.Services;
        _mockContext.Setup(c => c.InstanceServices).Returns(services);

        _mockRequest = new Mock<HttpRequestData>(_mockContext.Object);
        _function = new NegotiateFunction(_mockLogger.Object);
    }

    /// <summary>
    /// Test for the NegotiateFunction Run method.
    /// </summary>
    [Fact]
    public async Task Negotiate_ReturnsConnectionInfoInResponse()
    {
        // Fake connection info returned by SignalR binding
        var fakeConnectionInfo = new SignalRConnectionInfo {
            Url = "https://fake.service.com",
            AccessToken = "fake_access_token"
        };

        // Mock response
        var stream = new MemoryStream();
        var mockResponse = new Mock<HttpResponseData>(_mockContext.Object);
        mockResponse.Setup(r => r.Body).Returns(stream);
        mockResponse.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        mockResponse.SetupProperty(r => r.StatusCode);

        _mockRequest.Setup(r => r.CreateResponse())
                    .Returns(mockResponse.Object);

        // Call function
        HttpResponseData result =
            await _function.Negotiate(_mockRequest.Object, fakeConnectionInfo);

        // Assert status code
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        // Read JSON response body
        result.Body.Position = 0;
        using var reader = new StreamReader(result.Body);
        string jsonString = await reader.ReadToEndAsync();

        Dictionary<string, JsonElement> json =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString)!;

        Assert.Equal(fakeConnectionInfo.Url, json["Url"].GetString());
        Assert.Equal(fakeConnectionInfo.AccessToken, json["AccessToken"].GetString());

        // Logging verification
        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Negotiation request received.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}

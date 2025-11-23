/******************************************************************************
* Filename    = GroupFunctionsTests.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Unit tests for GroupFunctions Azure Functions.
*****************************************************************************/

using System.Net;
using System.Text;
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
/// Class containing unit tests for JoinGroup and LeaveGroup.
/// </summary>
public class GroupFunctionsTests
{
    // Mock objects and function instance
    private readonly Mock<ILogger<GroupFunctions>> _mockLogger;
    private readonly Mock<FunctionContext> _mockContext;
    private readonly Mock<HttpRequestData> _mockRequest;
    private readonly GroupFunctions _function;

    /// <summary>
    /// Constructor to set up the test environment.
    /// </summary>
    public GroupFunctionsTests()
    {
        _mockLogger = new Mock<ILogger<GroupFunctions>>();
        _mockContext = new Mock<FunctionContext>();

        IHost host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        IServiceProvider services = host.Services;
        _mockContext.Setup(c => c.InstanceServices).Returns(services);

        _mockRequest = new Mock<HttpRequestData>(_mockContext.Object);
        _function = new GroupFunctions(_mockLogger.Object);
    }

    /// <summary>
    /// Helper to set mock query parameters.
    /// </summary>
    private void SetupQuery(string? meetingId, string? userId)
    {
        var query = new System.Collections.Specialized.NameValueCollection
        {
            { "meetingId", meetingId },
            { "userId", userId }
        };

        _mockRequest.Setup(r => r.Query).Returns(query);
    }

    /// <summary>
    /// Helper to mock HttpResponseData for both join/leave actions.
    /// </summary>
    private HttpResponseData PrepareMockResponse()
    {
        var stream = new MemoryStream();
        var mockResponse = new Mock<HttpResponseData>(_mockContext.Object);
        mockResponse.SetupProperty(r => r.StatusCode);
        mockResponse.Setup(r => r.Body).Returns(stream);
        mockResponse.Setup(r => r.Headers).Returns(new HttpHeadersCollection());

        _mockRequest.Setup(r => r.CreateResponse()).Returns(mockResponse.Object);

        return mockResponse.Object;
    }

    /// <summary>
    /// Test successful JoinGroup.
    /// </summary>
    [Fact]
    public async Task JoinGroupSuccessTest()
    {
        SetupQuery("Test123", "User1");
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result =
            await _function.JoinGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.OK, result.HttpResponse!.StatusCode);

        result.HttpResponse.Body.Position = 0;
        string body = await new StreamReader(result.HttpResponse.Body).ReadToEndAsync();
        Assert.Equal("User User1 joined group Test123.", body);

        Assert.NotNull(result.GroupAction);
        Assert.Equal(SignalRGroupActionType.Add, result.GroupAction!.Action);
        Assert.Equal("Test123", result.GroupAction.GroupName);
        Assert.Equal("User1", result.GroupAction.UserId);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v.ToString()!.Contains("User User1 joining group Test123")),
                It.IsAny<Exception>(),
                (Func<object, Exception?, string>)It.IsAny<object>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// Teststing if JoinGroup returns BadRequest when meetingId missing.
    /// </summary>
    [Fact]
    public async Task JoinGroupMissingMeetingIdTest()
    {
        SetupQuery(null, "User1");
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result = await _function.JoinGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.BadRequest, result.HttpResponse!.StatusCode);

        result.HttpResponse.Body.Position = 0;
        string body = await new StreamReader(result.HttpResponse.Body).ReadToEndAsync();
        Assert.Equal("meetingId and userId are required.", body);

        Assert.Null(result.GroupAction);
    }

    /// <summary>
    /// Testing if JoinGroup returns BadRequest when userId missing.
    /// </summary>
    [Fact]
    public async Task JoinGroupMissingUserIdTest()
    {
        SetupQuery("Test123", null);
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result = await _function.JoinGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.BadRequest, result.HttpResponse!.StatusCode);
    }

    /// <summary>
    /// Test successful LeaveGroup.
    /// </summary>
    [Fact]
    public async Task LeaveGroupSuccessTest()
    {
        SetupQuery("Test123", "User1");
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result =
            await _function.LeaveGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.OK, result.HttpResponse!.StatusCode);

        result.HttpResponse.Body.Position = 0;
        string body = await new StreamReader(result.HttpResponse.Body).ReadToEndAsync();
        Assert.Equal("User User1 left group Test123.", body);

        Assert.NotNull(result.GroupAction);
        Assert.Equal(SignalRGroupActionType.Remove, result.GroupAction!.Action);
        Assert.Equal("Test123", result.GroupAction.GroupName);
        Assert.Equal("User1", result.GroupAction.UserId);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v.ToString()!.Contains("User User1 leaving group Test123")),
                It.IsAny<Exception>(),
                (Func<object, Exception?, string>)It.IsAny<object>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// Testing if LeaveGroup returns BadRequest when meetingId missing.
    /// </summary>
    [Fact]
    public async Task LeaveGroupMissingMeetingIdTest()
    {
        SetupQuery(null, "User1");
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result = await _function.LeaveGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.BadRequest, result.HttpResponse!.StatusCode);
        Assert.Null(result.GroupAction);
    }

    /// <summary>
    /// Testing if LeaveGroup returns BadRequest when userId missing.
    /// </summary>
    [Fact]
    public async Task LeaveGroupMissingUserIdTest()
    {
        SetupQuery("Test123", null);
        HttpResponseData response = PrepareMockResponse();

        GroupFunctions.GroupResponse result = await _function.LeaveGroup(_mockRequest.Object);

        Assert.Equal(HttpStatusCode.BadRequest, result.HttpResponse!.StatusCode);
    }
}

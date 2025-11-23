/******************************************************************************
* Filename    = GroupFunctions.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Azure Functions to manage SignalR group membership.
*****************************************************************************/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Communicator.Cloud.SignalR;

/// <summary>
/// Class to handle group join/leave actions for SignalR.
/// </summary>
public class GroupFunctions
{
    /// <summary>
    /// Logger instance for logging information.
    /// </summary>
    private readonly ILogger<GroupFunctions> _logger;

    /// <summary>
    /// Constructor to initialize the logger.
    /// </summary>
    /// <param name="logger">Used to instantiate logger</param>
    public GroupFunctions(ILogger<GroupFunctions> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Response datastructure to return SignalR group actions and HTTP response.
    /// </summary>
    public class GroupResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public SignalRGroupAction? GroupAction { get; set; }

        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    /// <summary>
    /// Add user to SignalR group based on meeting id and user id
    /// </summary>
    /// <param name="req">HTTP POST request containing meetingId and userId</param>
    /// <returns>A <see cref="GroupResponse"/> with group add action and HTTP response.</returns>
    [Function("JoinGroup")]
    public async Task<GroupResponse> JoinGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        // Extract meeting id and user id
        var query = req.Query;

        string? meetingId = query["meetingId"];
        string? userId = query["userId"];

        if (string.IsNullOrEmpty(meetingId) || string.IsNullOrEmpty(userId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("meetingId and userId are required.");
            return new GroupResponse { HttpResponse = bad };
        }

        _logger.LogInformation($"User {userId} joining group {meetingId}");

        // Create the SignalR group add action
        var action = new SignalRGroupAction(SignalRGroupActionType.Add)
        {
            GroupName = meetingId,
            UserId = userId
        };

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteStringAsync($"User {userId} joined group {meetingId}.");

        return new GroupResponse { GroupAction = action, HttpResponse = ok };
    }

    /// <summary>
    /// Remove a user from SignalR group based on meeting id and user id
    /// </summary>
    /// <param name="req">HTTP POST request containing meetingId and userId</param>
    /// <returns>A <see cref="GroupResponse"/> with group add action and HTTP response.</returns>
    [Function("LeaveGroup")]
    public async Task<GroupResponse> LeaveGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        // Extract meeting id and user id
        var query = req.Query;

        string? meetingId = query["meetingId"];
        string? userId = query["userId"];

        if (string.IsNullOrEmpty(meetingId) || string.IsNullOrEmpty(userId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("meetingId and userId are required.");
            return new GroupResponse { HttpResponse = bad };
        }

        _logger.LogInformation($"User {userId} leaving group {meetingId}");

        // Create the SignalR group remove action
        var action = new SignalRGroupAction(SignalRGroupActionType.Remove)
        {
            GroupName = meetingId,
            UserId = userId
        };

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteStringAsync($"User {userId} left group {meetingId}.");

        return new GroupResponse { GroupAction = action, HttpResponse = ok };
    }
}

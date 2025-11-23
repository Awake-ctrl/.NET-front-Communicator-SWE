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
    private readonly ILogger<GroupFunctions> _logger;

    public GroupFunctions(ILogger<GroupFunctions> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Response container to return SignalR group actions + HTTP response.
    /// </summary>
    public class GroupResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public SignalRGroupAction? GroupAction { get; set; }

        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    /***********************************************************************
     * JOIN GROUP
     ***********************************************************************/
    [Function("JoinGroup")]
    public async Task<GroupResponse> JoinGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
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

        var action = new SignalRGroupAction(SignalRGroupActionType.Add)
        {
            GroupName = meetingId,
            UserId = userId
        };

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteStringAsync($"User {userId} joined group {meetingId}.");

        return new GroupResponse { GroupAction = action, HttpResponse = ok };
    }

    /***********************************************************************
     * LEAVE GROUP
     ***********************************************************************/
    [Function("LeaveGroup")]
    public async Task<GroupResponse> LeaveGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
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

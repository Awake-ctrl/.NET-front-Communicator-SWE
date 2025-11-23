/******************************************************************************
* Filename    = NegotiateFunction.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Azure Function to handle SignalR negotiation requests.
*****************************************************************************/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Communicator.Cloud.SignalR;

/// <summary>
/// Class to handle SignalR negotiation requests.
/// </summary>
public class NegotiateFunction
{
    private readonly ILogger<NegotiateFunction> _logger;

    public NegotiateFunction(ILogger<NegotiateFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Function app endpoint to handle negotiation requests.
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="connectionInfo">Auto-generated SignalR connection info</param>
    [Function("negotiate")]
    public async Task<HttpResponseData> Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "meetingHub", UserId = "{userId}")] SignalRConnectionInfo connectionInfo)
    {
        _logger.LogInformation("Negotiation request received.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(connectionInfo);

        return response;
    }
}

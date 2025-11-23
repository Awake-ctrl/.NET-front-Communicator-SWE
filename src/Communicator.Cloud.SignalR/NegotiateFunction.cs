/******************************************************************************
* Filename    = NegotiateFunction.cs
* Author      = Nikhil S Thomas
* Product     = Comm-Uni-Cator
* Project     = SignalR Function App
* Description = Azure Function to handle SignalR negotiation requests.
*****************************************************************************/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.SignalRService;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;

namespace Communicator.Cloud.SignalR;

/// <summary>
/// Azure Function that handles SignalR negotiation and returns connection info.
/// </summary>
public class NegotiateFunction
{
    /// <summary>
    /// Logger instance for logging information
    /// </summary>
    private readonly ILogger<NegotiateFunction> _logger;

    /// <summary>
    /// Constructor to initialize the logger.
    /// </summary>
    /// <param name="logger">Used to instantiate logger</param>
    public NegotiateFunction(ILogger<NegotiateFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Response datastructure containing both SignalR output and HTTP response.
    /// </summary>
    public class NegotiateResponse
    {
        [SignalROutput(HubName = "meetingHub")]
        public object? SignalROutput { get; set; }

        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    /// <summary>
    /// HTTP-triggered function that generates SignalR connection info
    /// and automatically adds the user to a SignalR group.
    /// </summary>
    [Function("negotiate")]
    public async Task<NegotiateResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "meetingHub", UserId = "{meetingId}")]
        SignalRConnectionInfo connectionInfo)
    {
        // Extract meeting id from the query string
        NameValueCollection query = HttpUtility.ParseQueryString(req.Url.Query);
        string? meetingId = query["meetingId"];

        _logger.LogInformation($"Negotiation request received. MeetingId={meetingId}");

        // Create HTTP response containing SignalR URL, access token, and meeting ID
        HttpResponseData httpResponse = req.CreateResponse(HttpStatusCode.OK);
        await httpResponse.WriteAsJsonAsync(new
        {
            url = connectionInfo.Url,
            accessToken = connectionInfo.AccessToken,
            meetingId
        });

        // Create group action to add the user to a SignalR group matching the meeting ID
        var groupAddAction = new SignalRGroupAction(SignalRGroupActionType.Add)
        {
            UserId = meetingId,
            GroupName = meetingId
        };

        // Return both SignalR and HTTP responses
        return new NegotiateResponse
        {
            SignalROutput = new
            {
                connectionInfo,
                groupAddAction
            },
            HttpResponse = httpResponse
        };
    }
}

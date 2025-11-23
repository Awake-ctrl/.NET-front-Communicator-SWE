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
    [SignalRConnectionInfoInput(HubName = "meetingHub")] SignalRConnectionInfo connectionInfo)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string? meetingId = query["meetingId"];

            _logger.LogInformation($"Negotiation request received. MeetingId={meetingId}");

            var groupAction = new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                UserId = meetingId,
                GroupName = meetingId
            };

            var http = req.CreateResponse(HttpStatusCode.OK);
            await http.WriteAsJsonAsync(new
            {
                url = connectionInfo.Url,
                accessToken = connectionInfo.AccessToken,
                meetingId
            });

            // IMPORTANT: return array to satisfy output binding
            return new NegotiateResponse
            {
                SignalROutput = new object[] { connectionInfo, groupAction },
                HttpResponse = http
            };
        }
        catch (Exception ex)
        {
            // This reveals the true underlying problem
            _logger.LogError(ex, "NEGOTIATE FAILED: {Message}", ex.Message);

            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync(ex.ToString());
            return new NegotiateResponse
            {
                HttpResponse = resp
            };
        }
    }

}
